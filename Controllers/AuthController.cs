using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiVrEdu.Dto;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ApiVrEdu.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;

    public AuthController(IWebHostEnvironment env, UserManager<User> userManager, IConfiguration configuration,
        RoleManager<Role> roleManager)
    {
        _env = env;
        _userManager = userManager;
        _configuration = configuration;
        _roleManager = roleManager;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromForm] LoginDto model)
    {
        var user = await _userManager.FindByNameAsync(model.Username.ToLower());
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password.ToLower()))
            return Unauthorized();
        if (!user.IsActivated)
            return Unauthorized(new Response
            {
                Status = "Error",
                Message =
                    "Votre compte est desactive veillez demander consulter un administrateur afin qu'il l'active !"
            });
        var userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        authClaims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

        var token = GetToken(authClaims);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiration = token.ValidTo
        });
    }


    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(
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
        var userExists = await _userManager.FindByNameAsync(username.ToLower());
        if (userExists != null)
            return StatusCode(StatusCodes.Status500InternalServerError,
                new Response { Status = "Error", Message = "User already exists!" });

        User user = new()
        {
            Email = email.ToLower(),
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = username.ToLower(),
            LastName = lastname.ToLower(),
            FirstName = firstname.ToLower(),
            BirthDate = birthDate,
            Sex = sex,
            PhoneNumber = phoneNumber,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
        string? path = null;

        try
        {
            if (image != null) path = await FileManager.CreateFile(image, username, _env, new[] { "users" });
        }
        catch (Exception e)
        {
            return BadRequest(new Response { Status = "Error", Message = e.Message });
        }

        user.Image = path;
        var result = await _userManager.CreateAsync(user, password.ToLower());
        if (result.Succeeded) return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        FileManager.DeleteFile(path ?? "", _env);
        return StatusCode(StatusCodes.Status500InternalServerError,
            new Response
                { Status = "Error", Message = "User creation failed! Please check user details and try again." });
    }

    [HttpPost]
    [Route("register-admin")]
    public async Task<IActionResult> RegisterAdmin(
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
        var userExists = await _userManager.FindByNameAsync(username.ToLower());
        if (userExists != null)
            return StatusCode(StatusCodes.Status500InternalServerError,
                new Response { Status = "Error", Message = "User already exists!" });

        User user = new()
        {
            Email = email.ToLower(),
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = username.ToLower(),
            LastName = lastname.ToLower(),
            FirstName = firstname.ToLower(),
            BirthDate = birthDate,
            Sex = sex,
            PhoneNumber = phoneNumber,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
        string? path = null;

        try
        {
            if (image != null) path = await FileManager.CreateFile(image, username, _env, new[] { "users" });
        }
        catch (Exception e)
        {
            return BadRequest(new Response { Status = "Error", Message = e.Message });
        }

        user.Image = path;
        var result = await _userManager.CreateAsync(user, password.ToLower());
        if (!result.Succeeded)
            return StatusCode(StatusCodes.Status500InternalServerError,
                new Response
                    { Status = "Error", Message = "User creation failed! Please check user details and try again." });

        if (!await _roleManager.RoleExistsAsync(UserRole.Admin))
            await _roleManager.CreateAsync(new Role
            {
                Name = UserRole.Admin
            });
        if (!await _roleManager.RoleExistsAsync(UserRole.User))
            await _roleManager.CreateAsync(new Role
            {
                Name = UserRole.User
            });

        if (await _roleManager.RoleExistsAsync(UserRole.Admin))
            await _userManager.AddToRoleAsync(user, UserRole.Admin);
        return Ok(new Response { Status = "Success", Message = "User created successfully!" });
    }


    private JwtSecurityToken GetToken(IEnumerable<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? ""));

        var token = new JwtSecurityToken(
            _configuration["JWT:ValidIssuer"],
            _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }
}