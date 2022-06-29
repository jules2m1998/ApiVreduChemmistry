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
public class EffectController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IIncludableQueryable<Effect, TypeEffect> _query;

    public EffectController(DataContext context)
    {
        _context = context;
        _query = _context
            .Effects
            .Include(effect => effect.Product)
            .Include(effect => effect.Texture)
            .Include(effect => effect.TypeEffect);
    }

    [HttpPost]
    public async Task<ActionResult<Effect>> Create([FromBody] CEffectDto dto)
    {
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product is null)
            return BadRequest(new Response
            {
                Errors = new Dictionary<string, string>
                {
                    { "product", "Produit de la reaction inexistant !" }
                }
            });
        var type = await _context.TypeEffects.FindAsync(dto.TypeEffectId);
        if (type is null)
            return BadRequest(new Response
            {
                Errors = new Dictionary<string, string>
                {
                    { "type", "Type d'effet inexistant !" }
                }
            });
        var texture = await _context.Textures.FindAsync(dto.ProductId);

        var ef = new Effect
        {
            Color = dto.Color,
            Value = dto.Value,
            Operator = dto.Operator,
            Product = product,
            TypeEffect = type,
            Texture = texture
        };
        _context.Add(ef);
        await _context.SaveChangesAsync();

        var effect = await _query.AsNoTracking().FirstOrDefaultAsync(e => e.Id == ef.Id);

        return Created("", effect);
    }

    [HttpPut]
    public async Task<ActionResult<Effect>> Update([FromBody] UEffectDto dto)
    {
        var ef = await _query.FirstOrDefaultAsync(e => e.Id == dto.Id);
        if (ef is null) return NotFound();

        if (dto.ProductId is not null)
        {
            var pr = await _context.Products.FindAsync(dto.ProductId);
            if (pr is null)
                return BadRequest(new Response
                {
                    Errors = new Dictionary<string, string>
                    {
                        { "product", "Produit inexistant !" }
                    }
                });
            ef.Product = pr;
        }

        if (dto.TextureId is not null)
        {
            var pr = await _context.Textures.FindAsync(dto.ProductId);
            if (pr is null)
                return BadRequest(new Response
                {
                    Errors = new Dictionary<string, string>
                    {
                        { "texture", "Texture inexistant !" }
                    }
                });
            ef.Texture = pr;
        }

        if (dto.TypeEffectId is not null)
        {
            var pr = await _context.TypeEffects.FindAsync(dto.ProductId);
            if (pr is null)
                return BadRequest(new Response
                {
                    Errors = new Dictionary<string, string>
                    {
                        { "typeEffect", "Texture inexistant !" }
                    }
                });
            ef.TypeEffect = pr;
        }

        Tools.LoopToUpdateObject(ef, dto, new[] { "ProductId", "TextureId", "TypeEffectId", "id" });

        _context.Update(ef);
        await _context.SaveChangesAsync();

        return Created("", ef);
    }

    [HttpGet]
    public async Task<ActionResult<List<Effect>>> Get()
    {
        var efs = await _query.AsNoTracking().ToListAsync();
        return Ok(efs);
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<Effect>> Get(int id)
    {
        var ef = await _query.AsNoTracking().FirstOrDefaultAsync(effect => effect.Id == id);
        if (ef is null) return NotFound();
        return Ok(ef);
    }

    [HttpDelete]
    [Route("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var ef = await _query.AsNoTracking().FirstOrDefaultAsync(effect => effect.Id == id);
        if (ef is null) return NotFound();
        _context.Remove(ef);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}