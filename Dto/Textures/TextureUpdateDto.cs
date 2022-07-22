using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto.Textures;

public class TextureUpdateDto
{
    [Required(ErrorMessage = "Identifiant obligatoire !!")]
    public int Id { get; set; }

    public string? Name { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
    public int? GroupId { get; set; }
    public bool? IsActivated { get; set; }
    public bool? IsLiquid { get; set; }
}