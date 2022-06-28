using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto.Elements;

public class RegisterElementDto
{
    [Required(ErrorMessage = "Nom obligatoire !")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Symbol obligatoire !")]
    public string Symbol { get; set; } = string.Empty;

    [Required(ErrorMessage = "Couleur obligatoire !")]
    public string Color { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    [Required(ErrorMessage = "Nombre de masse obligatoire !")]
    public int MassNumber { get; set; }

    [Required(ErrorMessage = "Numero atomique obligatoire !")]
    public int AtomicNumber { get; set; }

    [Required(ErrorMessage = "Groupe obligatoire !")]
    public int GroupId { get; set; }

    [Required(ErrorMessage = "Type obligatoire !")]
    public int TypeId { get; set; }

    [Required(ErrorMessage = "Texture obligatoire !")]
    public int TextureId { get; set; }
}