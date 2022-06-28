using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto.Textures;

public class TextureRegisterDto
{
    [Required(ErrorMessage = "Nom de la texture obligatoire")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Image de la texture obligatoire")]
    public IFormFile Image { get; set; }

    [Required(ErrorMessage = "Le group de la texture est obligatoire")]
    public int GroupId { get; set; }
}