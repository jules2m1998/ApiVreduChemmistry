using System.ComponentModel.DataAnnotations;
using ApiVrEdu.Dto.Reactions;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models.Reactions;
using ApiVrEdu.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApiVrEdu.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReactionController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly ReactionRepository _repository;
    private readonly UserRepository _userRepository;

    public ReactionController(JwtService jwtService, UserRepository userRepository, ReactionRepository repository)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _repository = repository;
    }

    [HttpPost]
    [Route("")]
    public ActionResult<Reaction> Reaction(
        [Required(ErrorMessage = "Aucune donnee soumise !")]
        ReactionDto reactionDto
    )
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var userId = _jwtService.GetPayload(jwt ?? "");
        if (userId == null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");
        var user = _userRepository.GetOne((int)userId);
        if (user is null) return Unauthorized("Vous ne pouvez pas effectuer cette action !");

        try
        {
            var reaction = new Reaction
            {
                Description = reactionDto.Description,
                Name = reactionDto.Name,
                User = user
            };
            return Created("", _repository.Reaction(reaction, reactionDto.Reactants, reactionDto.Products));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("")]
    public ActionResult<List<Reaction>> Reaction()
    {
        return Ok(_repository.Reaction());
    }

    [HttpGet]
    [Route("{id:int}")]
    public ActionResult<Reaction> Reaction([Required] int id)
    {
        return Ok(_repository.Reaction(id));
    }
}