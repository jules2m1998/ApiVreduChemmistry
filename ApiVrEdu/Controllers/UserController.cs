using System.Security.Claims;
using ApiVrEdu.Dto;
using ApiVrEdu.Exceptions;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ApiVrEdu.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<User> _userManager;

    public UserController(IWebHostEnvironment env, UserManager<User> userManager)
    {
        _env = env;
        _userManager = userManager;
    }

    [HttpPut]
    public async Task<ActionResult<User>> UserUpdate([FromForm] UserUpdateDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return NotFound(new Response
            {
                Message = "Utilisateur inexistant !",
                Status = "Error"
            });

        foreach (var prop in dto.GetType().GetProperties())
        {
            if (prop.Name.ToLower() is ("id" or "image" or "OldPassword" or "NewPassword")) continue;
            var val = prop.GetValue(dto, null);
            if (val is null) continue;
            var type = user.GetType();
            var pr = type.GetProperty(prop.Name);
            pr?.SetValue(user, val);

            if (dto.GetType().GetProperties().Length > 1 && dto.GetType().GetProperties().Last().Equals(prop))
                user.UpdatedDate = DateTime.UtcNow;
        }

        if (dto.OldPassword is not null && dto.NewPassword is not null)
        {
            var rP = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            if (!rP.Succeeded)
                return BadRequest(new Response
                {
                    Status = "Error",
                    Errors = new Dictionary<string, string>
                    {
                        { "OldPassword", "Ancien mot de passe incorrect !" }
                    }
                });
        }

        if (dto.Image is not null)
            try
            {
                var path = await FileManager.CreateFile(dto.Image, user.UserName, _env, new[] { "users" });
                FileManager.DeleteFile(user.Image ?? "", _env);
                user.Image = path;
            }
            catch (Exception e)
            {
                return BadRequest(new Response { Status = "Error", Message = e.Message });
            }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return StatusCode(StatusCodes.Status500InternalServerError, new Response
            {
                Status = "Error",
                Message = "Veillez verifier vos informations et reessayer plus tard !"
            });
        return Ok(user);
    }

    [HttpPut]
    [Route("Admin")]
    [Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult<User>> UserUpdateAdmin([FromForm] UpdateUserAdminDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.Id.ToString());
        if (user is null)
            return NotFound();

        if (dto.BirthDate is not null) user.BirthDate = dto.BirthDate.Value.ToUniversalTime();

        if (dto.NewPassword is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var rP = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!rP.Succeeded)
                return BadRequest(new Response
                {
                    Message = "Ancien mot de passe incorrect !",
                    Status = "Error"
                });
        }

        var newUser = Tools.LoopToUpdateObject(user, dto, new[] { "id", "image", "BirthDate", "newPassword" });

        if (dto.Image is not null)
            try
            {
                var path = await FileManager.CreateFile(dto.Image, user.UserName, _env, new[] { "users" });
                FileManager.DeleteFile(user.Image ?? "", _env);
                newUser.Image = path;
            }
            catch (Exception e)
            {
                return BadRequest(new Response { Status = "Error", Message = e.Message });
            }

        try
        {
            await UpdateUse(user);
            return Ok(newUser);
        }
        catch (ExceptionResponse e)
        {
            return StatusCode(e.StatusCode ?? StatusCodes.Status500InternalServerError, new Response
            {
                Errors = e.Errors,
                Status = "Error"
            });
        }
    }

    [HttpGet, Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult<List<User>>> All()
    {
        var users = await _userManager.GetUsersInRoleAsync(UserRole.User);
        return Ok(users);
    }

    [HttpGet]
    [Authorize(Roles = UserRole.Admin)]
    [Route("{id:int}")]
    public async Task<ActionResult<User>> One(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        return Ok(user);
    }

    private async Task<User?> UpdateUse(User user)
    {
        try
        {
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded) return user;
            var errors = new Dictionary<string, string>();
            foreach (var identityError in result.Errors)
                switch (identityError.Code)
                {
                    case "DuplicateUserName":
                        errors.Add("username", "Cette utilisateur existe deja cree en un nouveau ou connectez vous !");
                        break;
                    case "DuplicateEmail":
                        errors.Add("email", "Cette adresse email est deja associe a un autre compte !");
                        break;
                }

            throw new ExceptionResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Errors = errors
            };
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is not PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } npgex)
                throw new ExceptionResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new Dictionary<string, string>
                    {
                        { "0", ex.InnerException?.Message ?? "" }
                    }
                };
            var constraintName = npgex.ConstraintName;
            if (constraintName != null && constraintName.ToLower().Contains("phone"))
                throw new ExceptionResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = new Dictionary<string, string>
                    {
                        { "phoneNumber", "Ce numero de telephone est deja associe a un autre compte !" }
                    }
                };

            throw new ExceptionResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Errors = new Dictionary<string, string>
                {
                    { "0", ex.Message }
                }
            };
        }
    }
}