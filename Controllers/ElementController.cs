using System.ComponentModel.DataAnnotations;
using ApiVrEdu.Data;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models.Elements;
using ApiVrEdu.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApiVrEdu.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ElementController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly JwtService _jwtService;
    private readonly ElementRepository _repository;
    private readonly TextureRepository _textureRepository;
    private readonly UserRepository _userRepository;

    public ElementController(ElementRepository repository, UserRepository userRepository, JwtService jwtService,
        TextureRepository textureRepository, IWebHostEnvironment env, DataContext context)
    {
        _repository = repository;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _textureRepository = textureRepository;
        _env = env;
        _context = context;
    }

    // Element
    [HttpPost]
    [Route("")]
    public async Task<ActionResult<Element>> Element(
        [Required(ErrorMessage = "Veillez renseigner s'il s'agit d'un atome ou d'une molecule")] [FromForm]
        TypeElement typeElement,
        [Required(ErrorMessage = "Nom de l'element obligatoire")] [FromForm]
        string name,
        [Required(ErrorMessage = "Symbole obligatoire")] [FromForm]
        string symbol,
        [Required(ErrorMessage = "Texture obligatoire")] [FromForm]
        int idTexture,
        [Required(ErrorMessage = "Type obligatoire")] [FromForm]
        int idType,
        [Required(ErrorMessage = "Groupe obligatoire")] [FromForm]
        int idGroup,
        [Required(ErrorMessage = "Image obligatoire")]
        IFormFile image
    )
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var texture = _textureRepository.Get(idTexture);
        if (texture is null) return BadRequest("Texture inexistante");
        var type = _repository.Type(idType);
        if (type is null) return BadRequest("Type inexistante");
        var group = _repository.Group(idGroup);
        if (group is null) return BadRequest("Group inexistante");

        string? path = null;

        try
        {
            path = await FileManager.CreateFile(image, user.UserName, _env, new[] { "elements" });
            var element = new Element
            {
                TypeElement = typeElement,
                Name = name,
                Symbol = symbol,
                Image = path,
                User = user,
                Type = type,
                Group = group,
                Texture = texture
            };

            return Created("", _repository.Element(element));
        }
        catch (Exception e)
        {
            FileManager.DeleteFile(path ?? "", _env);
            return BadRequest(e.Message);
        }
    }

    // Group
    [HttpPost]
    [Route("Group")]
    public ActionResult<ElementGroup> Group(
        [Required(ErrorMessage = "Nom obligatoire !")]
        string name
    )
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is { IsAdmin: false } or null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        var group = new ElementGroup
        {
            Name = name,
            User = user
        };

        return Created("", _repository.Group(group));
    }

    [HttpPut]
    [Route("Group")]
    public ActionResult<ElementGroup> Group(
        [Required(ErrorMessage = "Identifiant obligatoire")]
        int id,
        [Required(ErrorMessage = "Nom obligatoire !")]
        string name
    )
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        var group = _repository.Group(id);
        if (group == null) return BadRequest("Group inexistant !");
        if (group.User.Id != userId) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        group.Name = name;
        return Ok(_repository.UpdateGroup(group));
    }

    [HttpGet]
    [Route("oneGroup")]
    public ActionResult<ElementGroup> OneGroup([Required(ErrorMessage = "Identifiant obligatoire")] int id)
    {
        var group = _repository.Group(id);
        if (group is null) return BadRequest("Group innexistant");
        return Ok(group);
    }

    [HttpGet]
    [Route("Group")]
    public ActionResult<List<ElementGroup>> Group()
    {
        return Ok(_repository.Group());
    }


    [HttpDelete]
    [Route("Group")]
    public ActionResult DeleteGroup(
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

        var group = _repository.Group(id);
        if (group == null) return BadRequest("Group inexistant !");
        if (group.User.Id != userId) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        _repository.DeleteGroup(group);
        return NoContent();
    }

    [HttpPut]
    [Route("ActivateGroup")]
    public ActionResult<ElementGroup> Group(
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

        var group = _repository.Group(id);
        if (group == null) return BadRequest("Group inexistant !");
        if (group.User.Id != userId) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        group.IsActivated = true;

        return Ok(_repository.UpdateGroup(group));
    }

    [HttpPut]
    [Route("DeactivateGroup")]
    public ActionResult<ElementGroup> DeactivateGroup(
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

        var group = _repository.Group(id);
        if (group == null) return BadRequest("Group inexistant !");
        if (group.User.Id != userId) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        group.IsActivated = false;

        return Ok(_repository.UpdateGroup(group));
    }

    // Type
    [HttpPost]
    [Route("Type")]
    public ActionResult<ElementType> Type(
        [Required(ErrorMessage = "Nom obligatoire !")]
        string name
    )
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is { IsAdmin: false } or null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        var type = new ElementType
        {
            Name = name,
            User = user
        };

        return Created("", _repository.Type(type));
    }

    [HttpPut]
    [Route("Type")]
    public ActionResult<ElementType> Type(
        [Required(ErrorMessage = "Identifiant obligatoire")]
        int id,
        [Required(ErrorMessage = "Nom obligatoire !")]
        string name
    )
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is { IsAdmin: false } or null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var type = _repository.Type(id);
        if (type == null) return BadRequest("Type inexistant !");

        type.Name = name;

        return Ok(_repository.UpdateType(type));
    }

    [HttpPut]
    [Route("ActivateType")]
    public ActionResult<ElementType> UpdateType(
        [Required(ErrorMessage = "Identifiant obligatoire")]
        int id
    )
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is { IsAdmin: false } or null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var type = _repository.Type(id);
        if (type == null) return BadRequest("Type inexistant !");

        type.IsActivated = true;

        return Ok(_repository.UpdateType(type));
    }

    [HttpPut]
    [Route("DeactivateType")]
    public ActionResult<ElementType> DeactivateType(
        [Required(ErrorMessage = "Identifiant obligatoire")]
        int id
    )
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is { IsAdmin: false } or null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var type = _repository.Type(id);
        if (type == null) return BadRequest("Type inexistant !");

        type.IsActivated = false;

        return Ok(_repository.UpdateType(type));
    }

    [HttpDelete]
    [Route("Type")]
    public ActionResult Type(
        [Required(ErrorMessage = "Identifiant obligatoire")]
        int id
    )
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is { IsAdmin: false } or null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var type = _repository.Type(id);
        if (type == null) return BadRequest("Type inexistant !");

        _repository.DeleteType(type);
        return NoContent();
    }

    [HttpGet]
    [Route("Type")]
    public ActionResult<ElementType?> OneType(
        [Required(ErrorMessage = "Identifiant obligatoire")]
        int id
    )
    {
        var type = _repository.Type(id);
        if (type == null) return BadRequest("Type inexistant !");
        return Ok(type);
    }

    [HttpGet]
    [Route("OneType")]
    public ActionResult<ElementType> OneType()
    {
        return Ok(_repository.Type());
    }
}