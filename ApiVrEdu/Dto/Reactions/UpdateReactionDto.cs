using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto.Reactions;

public class UpdateReactionDto
{
    [Required(ErrorMessage = "Identifiant obligatoire !")]
    public int Id { set; get; }

    public string? Name { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;
    public bool? IsActivated { get; set; }

    public List<ComposeReaction>? ProductIds { get; set; }

    public List<ComposeReaction>? ReactantIds { get; set; }
}