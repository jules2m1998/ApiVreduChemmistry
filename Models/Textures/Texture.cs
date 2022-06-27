using ApiVrEdu.Models.Elements;

namespace ApiVrEdu.Models.Textures;

public class Texture : BaseModel, IModelImage
{
    public List<Element> Elements = new();
    public string Name { get; set; } = string.Empty;
    public TextureGroup Group { get; set; }
    public User User { get; set; }
    public string Image { get; set; } = string.Empty;
}