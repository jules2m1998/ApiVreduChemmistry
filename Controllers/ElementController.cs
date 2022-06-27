using System.Security.Claims;
using ApiVrEdu.Data;
using ApiVrEdu.Dto.Elements;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models.Elements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiVrEdu.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ElementController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IWebHostEnvironment _env;

    public ElementController(DataContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost]
    [Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult<Element>> Create([FromForm] RegisterElementDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(int.Parse(userId));

        if (user is null) return Unauthorized();

        var group = await _context.ElementGroups.FindAsync(dto.GroupId);
        if (group is null)
            return BadRequest(new Response
            {
                Errors = new Dictionary<string, string>
                {
                    {
                        "group",
                        "Group d'element inexistant !"
                    }
                }
            });
        var type = await _context.ElementTypes.FindAsync(dto.TypeId);
        if (type is null)
            return BadRequest(new Response
            {
                Errors = new Dictionary<string, string>
                {
                    {
                        "type",
                        "Type d'element chimique introuvable !"
                    }
                }
            });

        var texture = await _context.Textures.FindAsync(dto.TextureId);
        if (texture is null)
            return BadRequest(new Response
            {
                Errors = new Dictionary<string, string>
                {
                    {
                        "texture",
                        "Texture d'element chimique introuvable !"
                    }
                }
            });

        var element = new Element
        {
            Name = dto.Name,
            Symbol = dto.Symbol,
            Color = dto.Color,
            MassNumber = dto.MassNumber,
            AtomicNumber = dto.AtomicNumber,
            User = user,
            Group = group,
            Type = type,
            Texture = texture
        };
        string? path = null;
        try
        {
            path = await FileManager.CreateFile(dto.Image, user.UserName, _env, new[] { "elements" });
            element.Image = path;
        }
        catch (Exception e)
        {
            FileManager.DeleteFile(path ?? "", _env);
            return BadRequest(new Response
            {
                Status = "Error", Errors = new Dictionary<string, string>
                {
                    { "image", e.Message }
                }
            });
        }

        _context.Add(element);
        var id = await _context.SaveChangesAsync();

        element = await _context.Elements
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.Group)
            .Include(e => e.Type)
            .Include(e => e.Texture)
            .FirstOrDefaultAsync(e => e.Id == element.Id);

        return Created("", element);
    }
}