using System.Security.Claims;
using ApiVrEdu.Data;
using ApiVrEdu.Dto;
using ApiVrEdu.Dto.Elements;
using ApiVrEdu.Exceptions;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models.Elements;
using ApiVrEdu.Models.Textures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;

namespace ApiVrEdu.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ElementController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IIncludableQueryable<Element, Texture> _elements;
    private readonly IWebHostEnvironment _env;

    public ElementController(DataContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
        _elements = _context.Elements
            .Include(e => e.User)
            .Include(e => e.Group)
            .Include(e => e.Type)
            .Include(e => e.Children)
            .ThenInclude(children => children.Children)
            .Include(e => e.Texture);
    }

    [HttpPut]
    [Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult<Element>> Create([FromForm] UpdateElementDto dto)
    {
        var element = await _context
            .Elements
            .Include(e => e.Children)
            .FirstOrDefaultAsync(e => e.Id == dto.Id);

        if (element is null || element.Children.Count > 0) return NotFound();

        if (dto.GroupId is not null)
        {
            var group = await _context.ElementGroups.FindAsync(dto.GroupId);
            if (group is null)
                return BadRequest(new Response
                {
                    Errors = new Dictionary<string, string>
                    {
                        { "GroupId", "Groupe d'element introuvable" }
                    }
                });
            element.Group = group;
        }

        if (dto.TypeId is not null)
        {
            var type = await _context.ElementTypes.FindAsync(dto.TypeId);
            if (type is null)
                return BadRequest(new Response
                {
                    Errors = new Dictionary<string, string>
                    {
                        { "TypeId", "Type d'element introuvable" }
                    }
                });
            element.Type = type;
        }

        if (dto.TextureId is not null)
        {
            var texture = await _context.Textures.FindAsync(dto.TextureId);
            if (texture is null)
                return BadRequest(new Response
                {
                    Errors = new Dictionary<string, string>
                    {
                        { "TextureId", "Texture introuvable" }
                    }
                });
            element.Texture = texture;
        }

        var res = await dto.UpdateElement(element, _env);
        if (res is not null) return BadRequest(res);

        var r = _context.Elements.Update(element);
        await _context.SaveChangesAsync();

        return Ok(r.Entity);
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
            MassNumber = dto.MassNumber,
            AtomicNumber = dto.AtomicNumber,
            User = user,
            Group = group,
            Type = type,
            Texture = texture
        };
        if (dto.Image is not null)
        {
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

    [HttpGet]
    public async Task<ActionResult<List<Element>>> GetAll()
    {
        var all = await _elements.AsNoTracking().ToListAsync();

        return Ok(all);
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<ActionResult<Element>> GetOne(int id)
    {
        var one = await _elements.AsNoTracking().FirstOrDefaultAsync(element => element.Id == id);
        if (one is null) return NotFound();
        return Ok(one);
    }

    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = UserRole.Admin)]
    public async Task<ActionResult> Delete(int id)
    {
        var one = await _elements.AsNoTracking().FirstOrDefaultAsync(element => element.Id == id);
        if (one is null) return NotFound();
        FileManager.DeleteFile(one.Image ?? "", _env);
        _context.Remove(one);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost]
    [Route("molecule")]
    public async Task<ActionResult<Element>> CreateMolecule([FromForm] RegisterMoleculeDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(int.Parse(userId));
        if (user is null) return Unauthorized();
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

        List<Compose>? atoms;

        try
        {
            atoms = JsonConvert.DeserializeObject<List<Compose>>(dto.Atomes);
        }
        catch
        {
            return BadRequest(new Response
            {
                Errors = new Dictionary<string, string>
                {
                    { "atoms", "Fomat d'atome non supporte !" }
                }
            });
        }

        if (atoms is null or { Count: 0 })
            return BadRequest(new Response
            {
                Errors = new Dictionary<string, string>
                {
                    { "atoms", "Veillez renseigner des atomes pour la molecule !" }
                }
            });

        Element? element;

        try
        {
            var children = atoms.Select(atom =>
            {
                var elt = _context.Elements
                    .Include(e => e.Children)
                    .FirstOrDefault(e => e.Id == atom.Id);

                if (elt is null)
                    throw new ExceptionResponse
                    {
                        Errors = new Dictionary<string, string>
                        {
                            {
                                "atoms",
                                "L'un ou plusieurs des atomes renseigne inexistant veiller en renseigner des nouveau ou les creer !"
                            }
                        }
                    };
                if (elt.Children.Count > 0)
                    throw new ExceptionResponse
                    {
                        Errors = new Dictionary<string, string>
                        {
                            {
                                "atoms",
                                "L'un ou plusieurs de vos elemets sont des molecules !"
                            }
                        }
                    };

                return new ElementChildren
                {
                    Quantity = atom.Quantity,
                    Children = elt,
                    Position = atom.Position
                };
            }).ToList();

            element = new Element
            {
                Name = dto.Name,
                Texture = texture,
                User = user,
                Children = children
            };
        }
        catch (ExceptionResponse e)
        {
            return BadRequest(new Response
            {
                Errors = e.Errors
            });
        }

        if (dto.Image is not null)
        {
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
        }

        _context.Elements.Add(element);
        await _context.SaveChangesAsync();

        var el = await _elements.AsNoTracking().FirstOrDefaultAsync(e => e.Id == element.Id);
        return Created("", el);
    }

    [HttpPut]
    [Route("molecule")]
    public async Task<ActionResult<Element>> UpdateMolecule([FromForm] MoleculeUpdateDto dto)
    {
        var element = await _context
            .Elements
            .Include(e => e.Children)
            .FirstOrDefaultAsync(e => e.Id == dto.Id);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(int.Parse(userId));
        if (user is null) return Unauthorized();
        if (element is null) return NotFound();
        if (element.Children.Count == 0 || element.User.Id != user.Id) return Unauthorized();

        var newElt = Tools.LoopToUpdateObject(element, dto, new[] { "id", "image", "children" });
        if (dto.Image is not null)
        {
            string? path = null;
            try
            {
                path = await FileManager.CreateFile(dto.Image, newElt.Symbol ?? "update", _env, new[] { "elements" });
                FileManager.DeleteFile(element.Image ?? "", _env);
                newElt.Image = path;
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
        }

        if (dto.Atomes is not null)
        {
            List<Compose>? atoms;

            try
            {
                atoms = JsonConvert.DeserializeObject<List<Compose>>(dto.Atomes);
            }
            catch
            {
                return BadRequest(new Response
                {
                    Errors = new Dictionary<string, string>
                    {
                        { "atoms", "Fomat d'atome non supporte !" }
                    }
                });
            }

            if (atoms is null or { Count: 0 })
                return BadRequest(new Response
                {
                    Errors = new Dictionary<string, string>
                    {
                        { "atoms", "Veillez renseigner des atomes pour la molecule !" }
                    }
                });

            try
            {
                var children = atoms.Select(atom =>
                {
                    var elt = _context.Elements.Find(atom.Id);
                    if (elt is null)
                        throw new ExceptionResponse
                        {
                            Errors = new Dictionary<string, string>
                            {
                                {
                                    "atoms",
                                    "L'un ou plusieurs des atomes renseigne inexistant veiller en renseigner des nouveau ou les creer !"
                                }
                            }
                        };

                    return new ElementChildren
                    {
                        Quantity = atom.Quantity,
                        Children = elt,
                        Position = atom.Position
                    };
                }).ToList();

                newElt.Children.Clear();
                newElt.Children.AddRange(children);
            }
            catch (ExceptionResponse e)
            {
                return BadRequest(new Response
                {
                    Errors = e.Errors
                });
            }
        }

        _context.Update(newElt);
        await _context.SaveChangesAsync();

        return Ok(newElt);
    }

    [HttpDelete]
    [Route("molecule/{id:int}")]
    public async Task<ActionResult> DeleteMolecule(int id)
    {
        var element = await _context
            .Elements
            .Include(e => e.Children)
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(int.Parse(userId));
        if (user is null) return Unauthorized();
        if (element is null) return NotFound();
        if (element.Children.Count == 0 || element.User.Id != user.Id) return Unauthorized();

        _context.Remove(element);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}