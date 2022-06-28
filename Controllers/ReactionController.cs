using System.Security.Claims;
using ApiVrEdu.Data;
using ApiVrEdu.Dto.Reactions;
using ApiVrEdu.Exceptions;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models.Elements;
using ApiVrEdu.Models.Reactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace ApiVrEdu.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReactionController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IIncludableQueryable<Reaction, Element> _queryable;

    public ReactionController(DataContext context)
    {
        _context = context;
        _queryable = context
            .Reactions
            .Include(reaction => reaction.User)
            .Include(reaction => reaction.Products)
            .ThenInclude(product => product.Element)
            .Include(reaction => reaction.Reactants)
            .ThenInclude(reactant => reactant.Element);
    }

    [HttpPost]
    public async Task<ActionResult<Reaction>> Create([FromBody] RegisterReactionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(int.Parse(userId));
        if (user is null) return Unauthorized();
        if (dto.ProductIds.Count == 0)
            return BadRequest(new Response
            {
                Errors = new Dictionary<string, string>
                {
                    { "product", "Nombre de produit insuffisant pour une reaction chimique !" }
                }
            });
        if (dto.ReactantIds.Count == 0)
            return BadRequest(new Response
            {
                Errors = new Dictionary<string, string>
                {
                    { "reactants", "Nombre de reactif insuffisant pour une reaction chimique !" }
                }
            });

        try
        {
            var reactants = dto.ReactantIds.Select(compose =>
            {
                var elt = _context.Elements.Find(compose.Id);
                if (elt is null)
                    throw new ExceptionResponse
                    {
                        Errors = new Dictionary<string, string>
                        {
                            { "reactant", "Un ou plusieurs reactifs inexistant" }
                        }
                    };
                return new Reactant
                {
                    Element = elt,
                    Quantity = compose.Quantity
                };
            }).ToList();

            var products = dto.ProductIds.Select(compose =>
            {
                var elt = _context.Elements.Find(compose.Id);
                if (elt is null)
                    throw new ExceptionResponse
                    {
                        Errors = new Dictionary<string, string>
                        {
                            { "product", "Un ou plusieurs reactifs inexistant" }
                        }
                    };
                return new Product
                {
                    Element = elt,
                    Quantity = compose.Quantity
                };
            }).ToList();

            var reaction = new Reaction
            {
                Description = dto.Description,
                Name = dto.Name,
                Products = products,
                Reactants = reactants,
                User = user
            };
            _context.Add(reaction);
            await _context.SaveChangesAsync();

            var element = await _queryable.AsNoTracking().FirstOrDefaultAsync(r => r.Id == reaction.Id);
            if (element is null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response
                {
                    Errors = new Dictionary<string, string>
                    {
                        { "0", "Erreur internet veillez ressayer plus tard !" }
                    }
                });

            return Created("", element);
        }
        catch (ExceptionResponse e)
        {
            return BadRequest(new Response
            {
                Errors = e.Errors
            });
        }
    }

    [HttpPut]
    public async Task<ActionResult<Reaction>> Update([FromBody] UpdateReactionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(int.Parse(userId));
        var isAdmin = User.IsInRole(UserRole.Admin);
        if (user is null) return Unauthorized();
        var reaction = await _queryable.FirstOrDefaultAsync(r => r.Id == dto.Id);
        if (reaction is null) return NotFound();

        if (reaction.User.Id != user.Id && !isAdmin) return Unauthorized();

        var newReaction = Tools.LoopToUpdateObject(reaction, dto, new[] { "id", "ProductIds", "ReactantIds" });

        if (dto.ProductIds is not null)
        {
            if (dto.ProductIds.Count == 0)
                return BadRequest(new Response
                {
                    Errors = new Dictionary<string, string>
                    {
                        { "product", "Nombre de produit insuffisant pour une reaction chimique !" }
                    }
                });

            try
            {
                var products = dto.ProductIds.Select(compose =>
                {
                    var elt = _context.Elements.Find(compose.Id);
                    if (elt is null)
                        throw new ExceptionResponse
                        {
                            Errors = new Dictionary<string, string>
                            {
                                { "product", "Un ou plusieurs reactifs inexistant" }
                            }
                        };
                    return new Product
                    {
                        Element = elt,
                        Quantity = compose.Quantity
                    };
                }).ToList();
                newReaction.Products.Clear();
                newReaction.Products.AddRange(products);
            }
            catch (ExceptionResponse e)
            {
                return BadRequest(new Response
                {
                    Errors = e.Errors
                });
            }
        }

        if (dto.ReactantIds is not null)
        {
            if (dto.ReactantIds.Count == 0)
                return BadRequest(new Response
                {
                    Errors = new Dictionary<string, string>
                    {
                        { "reactant", "Nombre de produit insuffisant pour une reaction chimique !" }
                    }
                });

            try
            {
                var reactants = dto.ReactantIds.Select(compose =>
                {
                    var elt = _context.Elements.Find(compose.Id);
                    if (elt is null)
                        throw new ExceptionResponse
                        {
                            Errors = new Dictionary<string, string>
                            {
                                { "reactant", "Un ou plusieurs reactifs inexistant" }
                            }
                        };
                    return new Reactant
                    {
                        Element = elt,
                        Quantity = compose.Quantity
                    };
                }).ToList();
                newReaction.Reactants.Clear();
                newReaction.Reactants.AddRange(reactants);
            }
            catch (ExceptionResponse e)
            {
                return BadRequest(new Response
                {
                    Errors = e.Errors
                });
            }
        }

        _context.Update(newReaction);
        await _context.SaveChangesAsync();

        var r = await _queryable.AsNoTracking().FirstOrDefaultAsync(rr => rr.Id == newReaction.Id);

        return Ok(r);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<Reaction>>> Get()
    {
        var elts = await _queryable.AsNoTracking().ToListAsync();

        return Ok(elts);
    }

    [HttpGet]
    [Route("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Reaction>> Get(int id)
    {
        var reaction = await _queryable.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        if (reaction is null) return NotFound();

        return Ok(reaction);
    }

    [HttpDelete]
    [Route("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(int.Parse(userId));
        var isAdmin = User.IsInRole(UserRole.Admin);
        if (user is null) return Unauthorized();
        var reaction = await _queryable.FirstOrDefaultAsync(r => r.Id == id);
        if (reaction is null) return NotFound();

        if (reaction.User.Id != user.Id && !isAdmin) return Unauthorized();

        _context.Remove(reaction);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}