using System.ComponentModel.DataAnnotations;
using ApiVrEdu.Exceptions;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models.Textures;
using ApiVrEdu.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiVrEdu.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class TextureController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly JwtService _jwtService;
    private readonly TextureRepository _repository;
    private readonly UserRepository _userRepository;

    public TextureController(TextureRepository repository, JwtService jwtService, UserRepository userRepository,
        IWebHostEnvironment environment)
    {
        _repository = repository;
        _jwtService = jwtService;
        _userRepository = userRepository;
        _environment = environment;
    }

    // Texture Group Management
    [HttpPost]
    public ActionResult<TextureGroup> TextureGroup([Required(ErrorMessage = "Nom du group obligatoire")] string name)
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is not { IsAdmin: true, IsActivated: true })
            return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        var textureGroup = new TextureGroup
        {
            Name = name,
            User = user
        };

        try
        {
            return Created("Group cree !", _repository.CreateGroup(textureGroup));
        }
        catch (DbUpdateException e)
        {
            return BadRequest(ExceptionCatcher.CatchUniqueViolation(e, new UniqueViolation
            {
                Name = "name",
                Msg = "Ce group de texture existe deja !"
            }));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut]
    public ActionResult<TextureGroup> TextureGroup(
        [Required(ErrorMessage = "Identifiant obligatoire")]
        int id,
        [Required(ErrorMessage = "Nom du group obligatoire")]
        string name
    )
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is not { IsAdmin: true, IsActivated: true })
            return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        var textureGroup = _repository.GetOneGroup(id);
        if (textureGroup == null || textureGroup.User.Id != user.Id) return BadRequest("Group inexistant !");
        textureGroup.Name = name;
        try
        {
            return Ok(_repository.UpdateGroup(textureGroup));
        }
        catch (DbUpdateException e)
        {
            return BadRequest(ExceptionCatcher.CatchUniqueViolation(e, new UniqueViolation
            {
                Name = "name",
                Msg = "Ce group de texture existe deja !"
            }));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete]
    public ActionResult TextureGroup(
        [Required(ErrorMessage = "Identifiant obligatoire")]
        int id
    )
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is not { IsAdmin: true, IsActivated: true })
            return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        var textureGroup = _repository.GetOneGroup(id);
        if (textureGroup == null || textureGroup.User.Id != user.Id) return BadRequest("Group inexistant !");
        try
        {
            _repository.DeleteGroup(textureGroup);
            return NoContent();
        }
        catch (DbUpdateException e)
        {
            return BadRequest(ExceptionCatcher.CatchUniqueViolation(e, new UniqueViolation
            {
                Name = "name",
                Msg = "Ce group de texture existe deja !"
            }));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    public ActionResult<List<TextureGroup>> TextureGroup()
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        return Ok(_repository.GetAllGroups());
    }

    [HttpGet]
    public ActionResult<TextureGroup> OneTextureGroup([Required(ErrorMessage = "Identifiant obligatoire")] int id)
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var group = _repository.GetOneGroup(id);

        if (group is null) return BadRequest("Group inexistant");
        return Ok(group);
    }

    // Texture Management
    [HttpPost]
    public async Task<ActionResult<Texture>> Texture(
        [Required(ErrorMessage = "Le nom de la texture est obligatoire !")] [FromForm]
        string name,
        [Required(ErrorMessage = "La couleur de la texture est obligatoire !")] [FromForm]
        string color,
        [Required(ErrorMessage = "Le group de la texture est obligatoire !")] [FromForm]
        int groupId,
        [Required(ErrorMessage = "L'image de la texture est obligatoire !")]
        IFormFile image
    )
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var group = _repository.GetOneGroup(groupId);
        if (group is null) return Unauthorized("Veiller renseigner un group valide !");

        try
        {
            var path = await FileManager.CreateFile(image, user.UserName, _environment, new[] { "textures" });
            if (path == null) return BadRequest("Un probleme est survenu lors de la sauvegarde de l'image !");
            var texture = new Texture
            {
                Name = name,
                Image = path,
                Color = color,
                User = user,
                Group = group
            };
            return Created("", _repository.Create(texture));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut]
    public async Task<ActionResult<Texture>> Texture(
        [Required(ErrorMessage = "Identifiant obligatoire !")] [FromForm]
        int id,
        [FromForm] string? name,
        [FromForm] string? color,
        [FromForm] int? groupId,
        IFormFile? image
    )
    {
        var texture = _repository.Get(id);
        if (texture == null) return BadRequest("Texture ineexistante !");
        string?[] props = { name, color };
        var isNull = props.All(p => p == null);
        if (isNull && groupId == null && image == null) return BadRequest("Aucune information transmise !");
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        if (image != null)
        {
            FileManager.DeleteFile(texture.Image ?? "", _environment);
            var path = await FileManager.CreateFile(image, user.UserName, _environment, new[] { "textures" });
            texture.Image = path ?? "";
        }

        if (name != null) texture.Name = name;

        if (color != null) texture.Color = color;

        if (groupId != null)
        {
            var group = _repository.GetOneGroup((int)groupId);
            if (group == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

            texture.Group = group;
        }

        return Ok(_repository.Update(texture));
    }

    [HttpDelete]
    public ActionResult Texture(
        [Required(ErrorMessage = "Identifiant obligatoire")]
        int id
    )
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        var texture = _repository.Get(id);
        if (texture == null) return BadRequest("Texture inexistante");
        if (texture.User.Id != userId) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        _repository.Delete(texture);

        return NoContent();
    }

    [HttpGet]
    public ActionResult<Texture> OneTexture(
        [Required(ErrorMessage = "Identifiant obligatoire")]
        int id
    )
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        var texture = _repository.Get(id);
        if (texture == null) return BadRequest("Texture inexistante");
        if (texture.User.Id != userId) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        return Ok(texture);
    }
    
    [HttpGet]
    public ActionResult<List<Texture>> Texture()
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        return Ok(_repository.Get());
    }
    
}