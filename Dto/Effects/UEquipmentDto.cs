using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto.Effects;

public class UEquipmentDto
{
    [Required(ErrorMessage = "Nom obligatoire !")]
    public int Id { get; set; }

    public string? Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public IFormFile? File { get; set; }
    public int? TypeEffectId { get; set; }
    public bool? IsActivated { get; set; }
}