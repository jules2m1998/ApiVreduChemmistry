using ApiVrEdu.Models.Textures;

namespace ApiVrEdu.Dto.Textures;

public class TextureUpdateDto
{
    public string? Name { get; set; }
    public double? DisplacementScale { get; set; }
    public double? Roughness { get; set; }
    public double? Metalness { get; set; }
    public int? R { get; set; }
    public int? G { get; set; }
    public int? B { get; set; }
    public int? A { get; set; }

    public IFormFile? RoughnessMap { get; set; }
    public IFormFile? Map { get; set; }
    public IFormFile? NormalMap { get; set; }
    public IFormFile? DisplacementMap { get; set; }
    public IFormFile? AoMap { get; set; }
    public IFormFile? MetalnessMap { get; set; }
    public TextureType? TextureType { get; set; }
}