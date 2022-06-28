using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto.Reactions;

public class RegisterReactionDto
{
    [Required(ErrorMessage = "Nom de la reaction obligatoire !")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Produits obligatoires !")]
    public List<ComposeReaction> ProductIds { get; set; } = new();

    [Required(ErrorMessage = "Reactifs obligatoires !")]
    public List<ComposeReaction> ReactantIds { get; set; } = new();
}

public class ComposeReaction
{
    public int Id { get; set; }
    public int Quantity { get; set; }
}