using System.ComponentModel.DataAnnotations;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models;
using ApiVrEdu.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApiVrEdu.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly JwtService _jwtService;
    private readonly UserRepository _repository;

    public UserController(UserRepository repository, IWebHostEnvironment env, JwtService jwtService)
    {
        _repository = repository;
        _env = env;
        _jwtService = jwtService;
    }

    [HttpPut]
    public async Task<ActionResult<User>> Update(
        [Required(ErrorMessage = "Identifiant obligatoire")] [FromForm]
        int id,
        [FromForm] string? username,
        [FromForm] string? password,
        [FromForm] string? lastname,
        [FromForm] string? firstname,
        [FromForm] [EmailAddress(ErrorMessage = "L'adresse email n'est pas valid")]
        string? email,
        [FromForm] [Phone(ErrorMessage = "Le numero de telephone n'est pas valid")]
        string? phoneNumber,
        IFormFile? image,
        [FromForm] DateTime? birthDate,
        [FromForm] SexType? sex
    )
    {
        string?[] props = { username, password, lastname, firstname, email, phoneNumber };
        var allStringNull = props.All(s => s == null);
        if (allStringNull && image == null && birthDate == null && sex == null)
            return BadRequest("Aucune information transmise !");
        string? path = null;
        try
        {
            var user = _repository.GetOne(id);
            var jwt = Request.Cookies["jwt"];
            var token = _jwtService.Verify(jwt);
            var userId = int.Parse(token.Issuer);

            if (user == null) return BadRequest("Utilisateur inexistant");
            if (user.Id != userId)
            {
                var admin = _repository.GetOne(userId);
                if (admin is null or { IsAdmin: false })
                    return Unauthorized("Vous ne pouvez pas modiffier ce compte !");
            }

            if (username != null) user.UserName = username;

            if (password != null) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            if (lastname != null) user.LastName = lastname;

            if (firstname != null) user.FirstName = firstname;

            if (email != null) user.Email = email;

            if (phoneNumber != null) user.PhoneNumber = phoneNumber;

            if (image != null)
            {
                FileManager.DeleteFile(user.Image ?? "", _env);
                path = await FileManager.CreateFile(image, user.UserName, _env, new[] { "users" });
                user.Image = path;
            }

            if (birthDate != null) user.BirthDate = (DateTime)birthDate;

            if (sex != null) user.Sex = (SexType)sex;

            return Ok(_repository.Update(user));
        }
        catch (Exception e)
        {
            FileManager.DeleteFile(path ?? "", _env);
            return Unauthorized(e.Message);
        }
    }

    [HttpDelete]
    public ActionResult Delete(int id)
    {
        try
        {
            var user = _repository.GetOne(id);
            var jwt = Request.Cookies["jwt"];
            var token = _jwtService.Verify(jwt);
            var userId = int.Parse(token.Issuer);
            var admin = _repository.GetOne(userId);

            if (user == null) return BadRequest("Utilisateur introuvable !");
            if (admin is { IsAdmin: false } || user.IsAdmin)
                return Unauthorized("Vous ne pouvez pas supprimer cette utilisateur !");

            _repository.Delete(user);
            return NoContent();
        }
        catch
        {
            return Unauthorized();
        }
    }

    [HttpPut]
    public ActionResult<User> Activate([Required(ErrorMessage = "Identifiant obligatoire !")] int id)
    {
        var user = _repository.GetOne(id);
        if (user == null) return BadRequest("Utilisateur introuvable");
        if (user.IsActivated) return BadRequest("Utilisateur actif");
        var jwt = Request.Cookies["jwt"];
        var token = _jwtService.Verify(jwt);
        var userId = int.Parse(token.Issuer);
        var admin = _repository.GetOne(userId);
        if (admin is { IsAdmin: false } || user.IsAdmin) return Unauthorized("Vous ne pouvez pas activer ce compte !");

        user.IsActivated = true;
        return Ok(_repository.Update(user));
    }

    [HttpPut]
    public ActionResult<User> Deactivate([Required(ErrorMessage = "Identifiant obligatoire !")] int id)
    {
        var user = _repository.GetOne(id);
        if (user == null) return BadRequest("Utilisateur introuvable");
        if (!user.IsActivated) return BadRequest("Utilisateur inactif");
        var jwt = Request.Cookies["jwt"];
        var token = _jwtService.Verify(jwt);
        var userId = int.Parse(token.Issuer);
        var admin = _repository.GetOne(userId);
        if (admin is { IsAdmin: false } || user.IsAdmin) return Unauthorized("Vous ne pouvez pas activer ce compte !");

        user.IsActivated = false;
        return Ok(_repository.Update(user));
    }
}