using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto.Elements;

public class MoleculeUpdateDto
{
    [Required(ErrorMessage = "Identifiant obligatoire !")]
    public int Id { get; set; }

    public string? Name { get; set; } = string.Empty;

    public string? Color { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    public int? TextureId { get; set; }

    public string? Atomes { get; set; }
}