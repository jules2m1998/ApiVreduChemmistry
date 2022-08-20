using ApiVrEdu.Models.Effects;
using ApiVrEdu.Models.Elements;

namespace ApiVrEdu.Models.Reactions;

public class Product : BaseModel
{
    public int Quantity { get; set; }
    public Element Element { get; set; }
    public Reaction Reaction { get; set; }
    public List<Effect> Effects { get; set; }
}