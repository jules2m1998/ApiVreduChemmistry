using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto.Elements;

public class RegisterMoleculeDto
{
    [Required(ErrorMessage = "Nom obligatoire !")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Couleur obligatoire !")]
    public string Color { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    [Required(ErrorMessage = "Texture obligatoire !")]
    public int TextureId { get; set; }

    [Required(ErrorMessage = "Atomes de la molecule obligatoire !")]
    public string Atomes { get; set; }
}