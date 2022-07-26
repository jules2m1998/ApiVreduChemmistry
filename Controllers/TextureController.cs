using System.Security.Claims;
using ApiVrEdu.Data;
using ApiVrEdu.Dto.Textures;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models.Textures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiVrEdu.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TextureController : ControllerBase
{
    private readonly DataContext _context;

    public TextureController(DataContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<Texture>> Register([FromForm] TextureRegisterDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var id)) return Unauthorized();

        var user = await _context.Users.FindAsync(id);
        if (user is null) return Unauthorized();
        var texture = await dto.ToTexture(user);
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
            .FirstOrDefaultAsync(t => t.Id == id);

        if (texture is null) return NotFound();
        if (texture.User.Id != idUser) return Unauthorized();


        Tools.LoopToUpdateObject(texture, dto, new[] { "r", "g", "b", "a" });
        if (dto.A is not null) texture.Color.A = (int)dto.A;
        if (dto.R is not null) texture.Color.R = (int)dto.R;
        if (dto.G is not null) texture.Color.G = (int)dto.G;
        if (dto.B is not null) texture.Color.B = (int)dto.B;

        var txt = _context.Textures.Update(texture);
        await _context.SaveChangesAsync();

        return Ok(txt.Entity);
    }
}