using System.Security.Claims;
using ApiVrEdu.Data;
using ApiVrEdu.Dto.Effects;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models.Effects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace ApiVrEdu.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class EquipmentController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IIncludableQueryable<Equipment, TypeEffect?> _query;

    public EquipmentController(DataContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
        _query = _context
            .Equipments
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.TypeEffect);
    }

    [HttpPost]
    [Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult<Equipment>> Create([FromForm] CEquipmentDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return Unauthorized();

        var equipment = new Equipment
        {
            Name = dto.Name,
            Description = dto.Description,
            User = user
        };

        if (dto.TypeEffectId is not null)
        {
            var typeEffect = await _context.TypeEffects.FindAsync(dto.TypeEffectId);
            if (typeEffect is null)
                return BadRequest(new Response
                {
                    Errors = new Dictionary<string, string>
                    {
                        { "typeEffect", "Type d'effet inexistant !" }
                    }
                });
            equipment.TypeEffect = typeEffect;
        }

        string? path = null;

        try
        {
            path = await FileManager.CreateFile(dto.File, user.UserName, _env, new[] { Tools.Locations.Equipment });
            equipment.File = path ?? "";
        }
        catch (Exception e)
        {
            if (path != null) FileManager.DeleteFile(path, _env);
            return BadRequest(new Response
            {
                Status = "Error",
                Errors = new Dictionary<string, string>
                {
                    { "File", e.Message }
                }
            });
        }

        _context.Add(equipment);
        await _context.SaveChangesAsync();

        var nEquipment = await _context
            .Equipments
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.TypeEffect)
            .FirstOrDefaultAsync(e => e.Id == equipment.Id);

        return Created("", nEquipment);
    }

    [HttpPut]
    [Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult<Equipment>> Update([FromForm] UEquipmentDto dto)
    {
        var eq = await _query.FirstOrDefaultAsync(equipment => dto.Id == equipment.Id);
        if (eq is null) return NotFound();

        var nEq = Tools.LoopToUpdateObject(eq, dto, new[] { "id", "File" });
        if (dto.File is not null)
        {
            string? path = null;

            try
            {
                path = await FileManager.CreateFile(dto.File, "update2", _env, new[] { Tools.Locations.Equipment });
                FileManager.DeleteFile(nEq.File ?? "", _env);
                nEq.File = path ?? "";
            }
            catch (Exception e)
            {
                if (path != null) FileManager.DeleteFile(path, _env);
                return BadRequest(new Response
                {
                    Status = "Error",
                    Errors = new Dictionary<string, string>
                    {
                        { "File", e.Message }
                    }
                });
            }
        }

        _context.Update(nEq);
        await _context.SaveChangesAsync();

        return Ok(nEq);
    }

    [HttpDelete]
    [Authorize(Roles = UserRole.Admin)]
    [Route("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var eq = await _query.FirstOrDefaultAsync(equipment => id == equipment.Id);
        if (eq is null) return NotFound();

        FileManager.DeleteFile(eq.File, _env);
        _context.Remove(eq);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("{id:int}")]
    public async Task<ActionResult<Equipment>> Get(int id)
    {
        var eq = await _query.FirstOrDefaultAsync(equipment => equipment.Id == id);
        if (eq is null) return NotFound();

        return Ok(eq);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<Equipment>>> Get()
    {
        var eq = await _query.ToListAsync();
        return Ok(eq);
    }
}