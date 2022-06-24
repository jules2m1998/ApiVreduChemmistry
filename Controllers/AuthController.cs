using System.ComponentModel.DataAnnotations;
using ApiVrEdu.Dto;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models;
using ApiVrEdu.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ApiVrEdu.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly JwtService _jwtService;
    private readonly UserRepository _repository;

    public AuthController(UserRepository repository, IWebHostEnvironment env, JwtService jwtService)
    {
        _repository = repository;
        _env = env;
        _jwtService = jwtService;
    }

    [HttpPost]
    public async Task<ActionResult<User>> Register(
        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire !")] [FromForm]
        string username,
        [Required(ErrorMessage = "Le mot de passe est obligatoire !")] [FromForm]
        string password,
        [Required(ErrorMessage = "Le nom est obligatoire !")] [FromForm]
        string lastname,
        [Required(ErrorMessage = "Le prenom est obligatoire !")] [FromForm]
        string firstname,
        [Required] [FromForm] [EmailAddress(ErrorMessage = "L'adresse email n'est pas valid")]
        string email,
        [Required] [FromForm] [Phone(ErrorMessage = "Le numero de telephone n'est pas valid")]
        string phoneNumber,
        IFormFile? image,
        [Required(ErrorMessage = "La date de naissance est obligatoire !")] [FromForm]
        DateTime birthDate,
        [Required(ErrorMessage = "Le sexe est obligatoire !")] [FromForm]
        SexType sex
    )
    {
        string? path = null;
        try
        {
            if (image != null) path = await FileManager.CreateFile(image, username, _env, new[] { "users" });

            var user = _repository.Create(
                username,
                BCrypt.Net.BCrypt.HashPassword(password),
                lastname,
                firstname,
                email,
                sex,
                birthDate,
                path,
                phoneNumber
            );
            return Ok(user);
        }
        catch (DbUpdateException ex)
        {
            FileManager.DeleteFile(path ?? "", _env);
            if (ex.InnerException is not PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } npgex)
                return BadRequest(ex.Message);
            var constraintName = npgex.ConstraintName;
            if (constraintName != null && constraintName.ToLower().Contains("email"))
                return BadRequest("Cette addresse existe deja veillez utuliser une nouvelle !");
            if (constraintName != null && constraintName.ToLower().Contains("phone"))
                return BadRequest("Ce numero de telephone est deja utilise veillez utiliser un nouveau !");
            return BadRequest("Champs mal renseigne veilllez verifier vos informations !");
        }
        catch (Exception ex)
        {
            FileManager.DeleteFile(path ?? "", _env);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public ActionResult<User> Login(LoginDto dto)
    {
        var user = _repository.GetByUserName(dto.Username);
        if (user == null) return BadRequest(new { message = "Nom d'utilisateur ou mot de passe incorrect !" });
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword))
            return BadRequest(new { message = "Nom d'utilisateur ou mot de passe incorrect !" });

        var jwt = _jwtService.Generate(user.Id);
        // Add to cookies
        Response.Cookies.Append("jwt", jwt, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            MaxAge = DateTimeOffset.Now.AddDays(1).TimeOfDay
        });

        return Ok(new
        {
            message = "success"
        });
    }

    [HttpGet]
    public ActionResult Logout()
    {
        Response.Cookies.Delete("jwt");

        return Ok(new
        {
            message = "Success"
        });
    }

    [HttpGet]
    public ActionResult<User> CurrentUser()
    {
        try
        {
            var jwt = Request.Cookies["jwt"];
            var token = _jwtService.Verify(jwt);
            var userId = int.Parse(token.Issuer);
            var user = _repository.GetOne(userId);

            return Ok(user);
        }
        catch
        {
            return Unauthorized();
        }
    }
}