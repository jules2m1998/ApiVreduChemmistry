using System.ComponentModel.DataAnnotations.Schema;
using ApiVrEdu.Models.Effects;
using ApiVrEdu.Models.Elements;

namespace ApiVrEdu.Models.Textures;

public enum TextureType
{
    Liquid,
    Solid
}

public class Texture : BaseModel
{
    public string Name { get; init; } = string.Empty;
    public double DisplacementScale { get; set; } = 1.05;
    public double Roughness { get; set; } = 1;
    public double Metalness { get; set; } = 0;
    [Column(TypeName = "jsonb")] public Color Color { get; set; } = new();
    public string? RoughnessMap { get; set; }
    public string? Map { get; set; }
    public string? NormalMap { get; set; }
    public string? DisplacementMap { get; set; }
    public string? AoMap { get; set; }
    public string? MetalnessMap { get; set; }

    public TextureType TextureType { get; set; } = TextureType.Solid;

    public User User { get; init; }
    public List<Element> Elements { get; set; } = new();
    public List<Effect> Effects { get; set; } = new();
}

public class Color
{
    public int R { get; set; } = 255;
    public int G { get; set; } = 255;
    public int B { get; set; } = 255;
    public int A { get; set; } = 1;
}