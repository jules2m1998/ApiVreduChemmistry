using System.ComponentModel.DataAnnotations.Schema;
using ApiVrEdu.Models.Reactions;
using ApiVrEdu.Models.Textures;

namespace ApiVrEdu.Models.Elements;

public class Element : BaseModel, IModelImage
{
    [InverseProperty("Parent")] public ICollection<ElementChildren> Children = new List<ElementChildren>();

    [InverseProperty("Children")] public ICollection<ElementChildren> Parents = new List<ElementChildren>();

    public List<Product> Products = new();

    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;

    public User User { get; set; } = new();

    public ElementType Type { get; set; } = new();

    public ElementGroup Group { get; set; } = new();

    public Texture Texture { get; set; } = new();

    public string? Image { get; set; } = string.Empty;
}