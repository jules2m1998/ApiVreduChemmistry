using ApiVrEdu.Models.Elements;

namespace ApiVrEdu.Models.Reactions;

public class Product : BaseModel
{
    public int Quantity { get; set; }
    public Element Element { get; set; } = new();
    public Reaction Reaction { get; set; } = new();
}