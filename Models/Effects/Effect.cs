using ApiVrEdu.Models.Reactions;
using ApiVrEdu.Models.Textures;

namespace ApiVrEdu.Models.Effects;

public enum Operator
{
    Upper,
    Lower,
    Equals
}

public class Effect : BaseModel
{
    public float Value { get; set; }
    public Operator Operator { get; set; }
    public string? Color { get; set; }

    public Product Product { get; set; }
    public Texture? Texture { get; set; }
    public TypeEffect TypeEffect { get; set; }
}