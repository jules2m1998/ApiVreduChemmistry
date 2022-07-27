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
    private readonly UserManager<User> _userManager;

    public TextureController(DataContext context, IWebHostEnvironment env, UserManager<User> userManager)
    {
        _context = context;
        _env = env;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<ActionResult<Texture>> Register([FromForm] TextureRegisterDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var id)) return Unauthorized();

        var user = await _context.Users.FindAsync(id);
        if (user is null) return Unauthorized();

        Texture? parent = null;

        if (dto.ParentId is not null) parent = await _context.Textures.FindAsync(dto.ParentId);
        var texture = await dto.ToTexture(user, parent, _env);
        var tt = _context.Textures.Add(texture);
        await _context.SaveChangesAsync();
        return Created("", tt.Entity);
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<Texture>> GetOne(int id)
    {
        var texture = await _context
            .Textures
            .Include(t => t.User)
            .Include(t => t.Parent)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (texture is null) return NotFound();

        return Ok(texture);
    }

    [HttpPut]
    [Route("{id:int}")]
    public async Task<ActionResult<Texture>> Update(int id, [FromForm] TextureUpdateDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var idUser)) return Unauthorized();

        var texture = await _context
            .Textures
            .Include(t => t.User)
            .Include(t => t.Parent)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (texture is null) return NotFound();
        if (texture.User.Id != idUser) return Unauthorized();


        await dto.ToTexture(texture, _env);
        var txt = _context.Textures.Update(texture);
        await _context.SaveChangesAsync();

        return Ok(txt.Entity);
    }

    [HttpGet]
    public async Task<ActionResult<List<Texture>>> GetAll()
    {
        var textures = await _context
            .Textures
            .AsNoTracking()
            .Include(texture => texture.User)
            .Include(texture => texture.Parent)
            .ToListAsync();

        return Ok(textures);
    }

    [HttpDelete]
    [Route("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var idUser)) return Unauthorized();
        var texture = await _context
            .Textures
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (texture is null) return NotFound();
        var user = await _userManager.FindByIdAsync(idUser.ToString());
        if (user is null) return Unauthorized();

        if (texture.User.Id != idUser && !await _userManager.IsInRoleAsync(user, UserRole.Admin)) return Unauthorized();

        _context.Textures.Remove(texture);
        Tools.LoopToDeleteFiles(texture, _env);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}