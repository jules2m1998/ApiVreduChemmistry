using System.ComponentModel.DataAnnotations.Schema;

namespace ApiVrEdu.Models;

public enum TypeElement
{
    Atom,
    Molecule
}

public class Element : BaseModel
{
    [InverseProperty("Products")] public List<Reaction> ReactionsProducts = new();

    [InverseProperty("Reactants")] public List<Reaction> ReactionsReactants = new();

    public TypeElement TypeElement = TypeElement.Atom;
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; } = string.Empty;
    public List<Element> Elements { get; set; } = new();

    [InverseProperty("Elements")] public List<Element> Molecules { get; set; } = new();

    public User User { get; set; } = new();

    [InverseProperty("Elements")] public ElementType Type { get; set; } = new();

    [InverseProperty("Elements")] public ElementGroup Group { get; set; } = new();
}