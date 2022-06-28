using System.ComponentModel.DataAnnotations.Schema;

namespace ApiVrEdu.Models.Textures;

public class TextureGroup : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public User User { get; set; }
    [InverseProperty("Group")] public ICollection<Texture> Textures { get; set; } = new List<Texture>();
}