using System.Security.Claims;
using ApiVrEdu.Data;
using ApiVrEdu.Dto.Effects;
using ApiVrEdu.Exceptions;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models;
using ApiVrEdu.Models.Effects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace ApiVrEdu.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TypeEffectController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IIncludableQueryable<TypeEffect, User> _query;

    public TypeEffectController(DataContext context)
    {
        _context = context;
        _query = _context.TypeEffects.Include(effect => effect.User);
    }

    [HttpPost]
    [Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult<TypeEffect>> Create(CTypeEffectDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr is null || !int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user is null) return Unauthorized();

        var type = new TypeEffect
        {
            Name = dto.Name,
            Unity = dto.Unity,
            UnitySymbol = dto.UnitySymbol,
            User = user
        };

        _context.Add(type);
        await _context.SaveChangesAsync();

        var nType = await _query.FirstOrDefaultAsync(effect => effect.Id == type.Id);

        return Created("", nType);
    }

    [HttpPut]
    [Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult<TypeEffect>> Update(UTypeEffectDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr is null || !int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user is null) return Unauthorized();
        var oldType = await _query.FirstOrDefaultAsync(effect => effect.Id == dto.Id);
        if (oldType is null) return NotFound();

        var type = Tools.LoopToUpdateObject(oldType, dto, new[] { "id" });

        _context.Update(type);
        await _context.SaveChangesAsync();

        return Created("", type);
    }

    [HttpDelete]
    [Authorize(Roles = UserRole.Admin)]
    [Route("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var type = await _context.TypeEffects.FindAsync(id);
        if (type is null) return NotFound();
        _context.Remove(type);
        await _context.SaveChangesAsync();


        return NoContent();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<TypeEffect>>> All()
    {
        var types = await _query
            .Include(effect => effect.Equipments)
            .AsNoTracking()
            .ToListAsync();
        return Ok(types);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("{id:int}")]
    public async Task<ActionResult<TypeEffect>> One(int id)
    {
        var type = await _query
            .Include(effect => effect.Equipments)
            .AsNoTracking()
            .FirstOrDefaultAsync(effect => effect.Id == id);
        if (type is null) return NotFound();
        return Ok(type);
    }

    [HttpPut]
    [Authorize(Roles = UserRole.Admin)]
    [Route("equipment")]
    public async Task<ActionResult<TypeEffect>> AddEquipment([FromBody] EquipmentUpdate update)
    {
        var type = await _query.FirstOrDefaultAsync(effect => effect.Id == update.Id);
        if (type is null) return NotFound();
        try
        {
            var equipments = update.Ids.Select(i =>
            {
                var eq = _context.Equipments.Find(i);
                if (eq is null)
                    throw new ExceptionResponse
                    {
                        Errors = new Dictionary<string, string>
                        {
                            { "id", "Un ou plusieurs des elements envoye n'existent pas !" }
                        }
                    };

                return eq;
            }).ToList();

            type.Equipments.AddRange(equipments);
        }
        catch (ExceptionResponse e)
        {
            return BadRequest(new Response
            {
                Errors = e.Errors
            });
        }

        _context.Update(type);
        await _context.SaveChangesAsync();

        return Ok(type);
    }
}

public class EquipmentUpdate
{
    public List<int> Ids { get; set; }
    public int Id { get; set; }
}