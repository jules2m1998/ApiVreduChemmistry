using ApiVrEdu.Models.Effects;
using ApiVrEdu.Models.Elements;

namespace ApiVrEdu.Models.Textures;

public class Texture : BaseModel, IModelImage
{
    public string Name { get; set; } = string.Empty;
    public TextureGroup Group { get; set; }
    public User User { get; set; }

    public List<Element> Elements { get; set; } = new();
    public List<Effect> Effects { get; set; } = new();
    public string Image { get; set; } = string.Empty;
}