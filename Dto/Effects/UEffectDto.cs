using System.ComponentModel.DataAnnotations;
using ApiVrEdu.Models.Effects;

namespace ApiVrEdu.Dto.Effects;

public class UEffectDto
{
    [Required(ErrorMessage = "Identifiant obligatoire !")]
    public int Id { get; set; }

    public float? Value { get; set; }
    public Operator? Operator { get; set; }
    public int? ProductId { get; set; }
    public int? TypeEffectId { get; set; }

    public string? Color { get; set; }
    public int? TextureId { get; set; }
    public bool? IsActivated { get; set; }
}