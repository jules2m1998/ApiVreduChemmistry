using System.ComponentModel.DataAnnotations;
using ApiVrEdu.Models.Effects;

namespace ApiVrEdu.Dto.Effects;

public class CEffectDto
{
    [Required(ErrorMessage = "Valeur de l'effet obligatoire !")]
    public float Value { get; set; }

    [Required(ErrorMessage = "Operateur de comparaison oblgatoire")]
    public Operator Operator { get; set; }

    public int ProductId { get; set; }
    public int TypeEffectId { get; set; }

    public string? Color { get; set; }
    public int? TextureId { get; set; }
}