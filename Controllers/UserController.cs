using System.Security.Claims;
using ApiVrEdu.Dto;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
                    Message = "Ancien mot de passe incorrect !",
                    Status = "Error"
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
                    Message = "Ancien mot de passe incorrect !",
                    Status = "Error"
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
}