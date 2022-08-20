using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto.Effects;

public class CEquipmentDto
{
    [Required(ErrorMessage = "Nom obligatoire !")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descriptiom obligatoire !")]
    public string Description { get; set; } = string.Empty;

    public bool IsConstraint { get; set; } = false;

    [Required(ErrorMessage = "Fichier obligatoire !")]
    public IFormFile File { get; set; }

    public int? TypeEffectId { get; set; }
}