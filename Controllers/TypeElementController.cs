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
[Route("api/[controller]")]
[Authorize]
public class TypeElementController : ControllerBase
{
    private readonly DataContext _context;

    public TypeElementController(DataContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult<ElementType>> Register([FromBody] string name)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(int.Parse(userId));

        if (user is null) return Unauthorized();
        var type = new ElementType
        {
            Name = name,
            User = user
        };
        _context.Add(type);
        var id = await _context.SaveChangesAsync();
        var typeCreated = await _context.ElementTypes.FindAsync(id);

        if (typeCreated == null)
            return StatusCode(StatusCodes.Status500InternalServerError, new Response
            {
                Errors = new Dictionary<string, string>
                {
                    {
                        "0",
                        "Une erreur s'est produite lors de la creation de l'element veillez verifier vos informations et reessayer plus tard !"
                    }
                }
            });
        return Created("", typeCreated);
    }

    [HttpPut]
    [Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult<ElementType>> Update([FromBody] UpdateDto dto)
    {
        var type = await _context.ElementTypes.FindAsync(dto.Id);
        if (type is null) return NotFound();

        Tools.LoopToUpdateObject(type, dto, new[] { "id" });
        _context.Update(type);
        await _context.SaveChangesAsync();

        return type;
    }

    [HttpDelete]
    [Authorize(Roles = UserRole.Admin)]
    [Route("{id:int}")]
    public async Task<ActionResult<ElementType>> Delete(int id)
    {
        var type = await _context.ElementTypes.FindAsync(id);

        if (type is null) return NotFound();

        _context.Remove(type);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<ElementType>>> GetAll()
    {
        var types = await _context.ElementTypes.Include(type => type.User).AsNoTracking().ToListAsync();
        return Ok(types);
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<ElementType>> GetOne(int id)
    {
        var type = await _context.ElementTypes.Include(elementType => elementType.User)
            .FirstOrDefaultAsync(elementType => elementType.Id == id);
        if (type is null) return NotFound();

        return Ok(type);
    }

    [HttpGet]
    [Route("user")]
    public async Task<ActionResult<List<ElementType>>> ByUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var type = await _context.ElementTypes.Include(elementType => elementType.User)
            .Where(elementType => elementType.User.Id == int.Parse(userId)).ToListAsync();
        return Ok(type);
    }
}