using System.ComponentModel.DataAnnotations.Schema;
using ApiVrEdu.Models.Reactions;
using ApiVrEdu.Models.Textures;

namespace ApiVrEdu.Models.Elements;

public class Element : BaseModel, IModelImage
{
    public string Name { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public int? MassNumber { get; set; }
    public int? AtomicNumber { get; set; }

    public User User { get; set; }

    public ElementType? Type { get; set; }

    public ElementGroup? Group { get; set; }

    public Texture Texture { get; set; }

    public List<Product> Products { get; set; }

    public string Description { get; set; } = string.Empty;

    [InverseProperty("Parent")] public List<ElementChildren> Children { get; set; }

    [InverseProperty("Children")] public List<ElementChildren>? Parents { get; set; }
    public string Image { get; set; } = string.Empty;
}