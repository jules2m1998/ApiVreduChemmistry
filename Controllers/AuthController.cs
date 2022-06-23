using System.ComponentModel.DataAnnotations;
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
    private readonly UserRepository _repository;

    public AuthController(UserRepository repository, IWebHostEnvironment env)
    {
        _repository = repository;
        _env = env;
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
            return BadRequest(ex.Message);
        }
    }
}