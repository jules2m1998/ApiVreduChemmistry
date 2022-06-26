using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto.Reactions;

public class ReactantDto
{
    [Required(ErrorMessage = "Quantite des reactifs tousse obligatoire")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Element des reactifs tousse obligatoire")]
    public int ElementId { get; set; }
}