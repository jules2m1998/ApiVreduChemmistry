using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto.Reactions;

public class ReactionDto
{
    [Required(ErrorMessage = "Nom ogligatoire !")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<ReactantDto> Reactants { get; set; } = new();
    public List<ProductDto> Products { get; set; } = new();
}