using ApiVrEdu.Models.Elements;

namespace ApiVrEdu.Models;

public class Reaction : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;

    public User User { get; set; } = new();

    public List<Element> Products { get; set; } = new();
    public List<Element> Reactants { get; set; } = new();
}