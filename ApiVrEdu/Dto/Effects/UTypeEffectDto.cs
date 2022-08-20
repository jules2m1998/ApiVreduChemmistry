using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto.Effects;

public class UTypeEffectDto
{
    [Required(ErrorMessage = "Identifiant obligatoire !")]
    public int Id { get; set; }

    public string? Name { get; set; }
    public string? Unity { get; set; }
    public string? UnitySymbol { get; set; }
    public bool? IsActivated { get; set; }
}