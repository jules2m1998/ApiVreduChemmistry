using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ApiVrEdu.Data;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models;
using ApiVrEdu.Models.Textures;
using ApiVrEdu.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiVrEdu.Controllers;

[ApiController, Authorize, Route("api/[controller]")]
public class TextureGroupController : ControllerBase
{
    private readonly DataContext _context;
    private readonly UserManager<User> _manager;
    private readonly TextureRepository _repository;

    public TextureGroupController(TextureRepository repository, UserManager<User> manager, DataContext context)
    {
        _repository = repository;
        _manager = manager;
        _context = context;
    }

    [HttpPost, Route(""), Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult<TextureGroup>> Group(
        [Required(ErrorMessage = "Nom obligatoire !")] [FromBody]
        string name)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _manager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        var group = new TextureGroup
        {
            Name = name,
            User = user
        };
        return Created("", _repository.CreateGroup(group));
    }

    [HttpGet, Route("user"), Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult<TextureGroup>> ByUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.Include(user1 => user1.TextureGroups)
            .FirstOrDefaultAsync(user1 => user1.Id == int.Parse(userId));
        if (user is null) return BadRequest();

        return Ok(user.TextureGroups);
    }

    [HttpGet, Route("")]
    public async Task<ActionResult<List<TextureGroup>>> GetAll()
    {
        var textures = await _context.TextureGroups.Include(group => group.User).ToListAsync();

        return Ok(textures);
    }

    [HttpGet, Route("{id:int}")]
    public async Task<ActionResult<TextureGroup>> GetOne(int id)
    {
        var texture = await _context.TextureGroups.Include(group => group.User)
            .FirstOrDefaultAsync(group => group.Id == id);

        if (texture is null) return NotFound();
        return Ok(texture);
    }

    [HttpPut, Authorize(Roles = UserRole.Admin), Route("{id:int}")]
    public async Task<ActionResult<TextureGroup>> Update(int id, string name)
    {
        var group = await _context.TextureGroups.FirstOrDefaultAsync(textureGroup => textureGroup.Id == id);
        if (group is null) return NotFound();

        group.Name = name;
        await _context.SaveChangesAsync();
        return Ok(group);
    }

    [HttpDelete, Authorize(Roles = UserRole.Admin), Route("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var group = await _context.TextureGroups.FirstOrDefaultAsync(textureGroup => textureGroup.Id == id);
        if (group is null) return NotFound();

        _context.Remove(group);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}