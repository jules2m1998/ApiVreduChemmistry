using System.Security.Claims;
using ApiVrEdu.Data;
using ApiVrEdu.Dto.Textures;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models;
using ApiVrEdu.Models.Textures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiVrEdu.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TextureController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<User> _manager;

    public TextureController(DataContext context, UserManager<User> manager, IWebHostEnvironment env)
    {
        _context = context;
        _manager = manager;
        _env = env;
    }

    [HttpPost]
    [Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult<Texture>> Create([FromForm] TextureRegisterDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _manager.FindByIdAsync(userId);
        if (user is null)
            return BadRequest(new Response
            {
                Status = "Errror",
                Errors = new Dictionary<string, string>
                {
                    { "0", "Utilisateur inexistant !" }
                }
            });

        var group = await _context.TextureGroups.FindAsync(dto.GroupId);

        if (group is null)
            return BadRequest(new Response
            {
                Status = "Errror",
                Errors = new Dictionary<string, string>
                {
                    { "0", "group de texture inexistant !" }
                }
            });

        var texture = new Texture
        {
            Name = dto.Name,
            Group = group,
            User = user
        };

        string? path = null;
        try
        {
            path = await FileManager.CreateFile(dto.Image, user.UserName, _env, new[] { "textures" });
            texture.Image = path ?? "";
            _context.Add(texture);
            var id = await _context.SaveChangesAsync();
            texture = await _context.Textures.AsNoTracking().FirstOrDefaultAsync(t => t.Id == texture.Id);
            return Created("", texture);
        }
        catch (Exception e)
        {
            if (path != null) FileManager.DeleteFile(path, _env);
            return BadRequest(new Response
            {
                Status = "Error",
                Errors = new Dictionary<string, string>
                {
                    { "image", e.Message }
                }
            });
        }
    }

    [HttpPut]
    [Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult<Texture>> Update([FromForm] TextureUpdateDto dto)
    {
        var texture = await _context.Textures.Include(t => t.User).AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == dto.Id);
        if (texture is null) return NotFound();

        if (dto.Image != null)
        {
            string? path = null;
            try
            {
                path = await FileManager.CreateFile(dto.Image, texture.User.UserName, _env, new[] { "textures" });
                if (path is not null)
                {
                    FileManager.DeleteFile(texture.Image, _env);
                    texture.Image = path;
                }
            }
            catch (Exception e)
            {
                FileManager.DeleteFile(path ?? "", _env);
                return BadRequest(new Response
                {
                    Errors = new Dictionary<string, string>
                    {
                        { "image", e.Message }
                    }
                });
            }
        }

        var newTexture = Tools.LoopToUpdateObject(texture, dto, new[] { "id", "image" });
        _context.Update(newTexture);
        await _context.SaveChangesAsync();

        return Ok(texture);
    }

    [HttpGet]
    public async Task<ActionResult<List<Texture>>> All()
    {
        return Ok(await _context.Textures.AsNoTracking().Include(texture => texture.User)
            .Include(texture => texture.Group).ToListAsync());
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<Texture>> One(int id)
    {
        var texture = await _context.Textures.AsNoTracking().Include(t => t.Group).Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (texture == null) return NotFound();
        return Ok(texture);
    }

    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult> Delete(int id)
    {
        var texture = await _context.Textures.AsNoTracking().Include(t => t.Group).Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (texture == null) return NotFound();
        FileManager.DeleteFile(texture.Image, _env);
        _context.Remove(texture);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet]
    [Route("group/{id:int}")]
    public ActionResult<List<Texture>> Group(int id)
    {
        var textures = _context.Textures
            .Include(texture => texture.User)
            .Include(texture => texture.Group)
            .AsNoTracking()
            .Where(t => t.Group.Id == id).ToList();
        return Ok(textures);
    }

    [HttpGet]
    [Route("user")]
    public ActionResult<List<Texture>> ByUser()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id is null)
            return BadRequest(new Response
            {
                Errors = new Dictionary<string, string>
                {
                    { "0", "Veillez verifier que vous etes bien authentifie avant de continuer !!" }
                },
                Status = "Error"
            });

        var textures = _context.Textures
            .Include(texture => texture.User)
            .Include(texture => texture.Group)
            .AsNoTracking()
            .Where(t => t.User.Id == int.Parse(id)).ToList();
        return Ok(textures);
    }
}