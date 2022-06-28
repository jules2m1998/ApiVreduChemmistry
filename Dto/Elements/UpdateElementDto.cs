using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto.Elements;

public class UpdateElementDto
{
    [Required(ErrorMessage = "Identifiant obligatoire !")]
    public int Id { get; set; }

    public string? Name { get; set; } = string.Empty;

    public string? Symbol { get; set; } = string.Empty;

    public string? Color { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    public int? MassNumber { get; set; }

    public int? AtomicNumber { get; set; }

    public int? GroupId { get; set; }

    public int? TypeId { get; set; }

    public int? TextureId { get; set; }

    public bool? IsActivated { get; set; }
}