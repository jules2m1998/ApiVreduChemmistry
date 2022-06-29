using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto.Effects;

public class CTypeEffectDto
{
    [Required(ErrorMessage = "Nom du type obligatoire")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Unite du type obligatoire")]
    public string Unity { get; set; } = string.Empty;

    [Required(ErrorMessage = "Symbole de l'unite obligatoire")]
    public string UnitySymbol { get; set; } = string.Empty;
}