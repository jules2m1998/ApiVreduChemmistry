namespace ApiVrEdu.Models.Reactions;

public class Reaction : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;

    public User User { get; set; } = new();

    public virtual List<Product> Products { get; set; } = new();
    public virtual List<Reactant> Reactants { get; set; } = new();
}