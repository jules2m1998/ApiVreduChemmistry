using System.ComponentModel.DataAnnotations;
using ApiVrEdu.Exceptions;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models;
using ApiVrEdu.Models.Textures;

namespace ApiVrEdu.Dto.Textures;

public class TextureRegisterDto
{
    private IWebHostEnvironment _env;

    [Required(ErrorMessage = "Nom de la texture obligatoire")]
    public string Name { get; set; } = string.Empty;

    public double? DisplacementScale { get; set; } = 1.05;
    public double? Roughness { get; set; } = 1;
    public double? Metalness { get; set; } = 0;
    public int? R { get; set; }
    public int? G { get; set; }
    public int? B { get; set; }
    public float? A { get; set; }

    public object? RoughnessMap { get; set; }
    public IFormFile? Map { get; set; }
    public IFormFile? NormalMap { get; set; }
    public IFormFile? DisplacementMap { get; set; }
    public IFormFile? AoMap { get; set; }
    public IFormFile? MetalnessMap { get; set; }
    public TextureType? TextureType { get; set; }
    public int? ParentId { get; set; }

    public async Task<Texture> ToTexture(User user, Texture? parent, IWebHostEnvironment env)
    {
        _env = env;
        var text = new Texture
        {
            User = user,
            Name = Name,
            RoughnessMap = await FileToString(RoughnessMap),
            Map = await FileToString(Map),
            NormalMap = await FileToString(NormalMap),
            DisplacementMap = await FileToString(DisplacementMap),
            AoMap = await FileToString(AoMap),
            MetalnessMap = await FileToString(MetalnessMap),
            Color = R is not null && G is not null && B is not null && A is not null
                ? new Color
                {
                    R = (int)R,
                    G = (int)G,
                    B = (int)B,
                    A = (float)A
                }
                : new Color(),
            Parent = parent
        };
        if (DisplacementScale is not null) text.DisplacementScale = (double)DisplacementScale;
        if (Roughness is not null) text.Roughness = (double)Roughness;
        if (Metalness is not null) text.Metalness = (double)Metalness;
        if (TextureType is not null) text.TextureType = (TextureType)TextureType;

        return text;
    }

    private async Task<string?> FileToString(object? file)
    {
        if (file is null) return null;
        if (file is not IFormFile fl) return file.ToString();

        string[] acceptExits = { "png", "jpg", "jpeg" };
        if (!acceptExits.Contains(FileManager.GetExtension(fl)))
            throw new ExceptionResponse
            {
                Errors = new Dictionary<string, string>
                {
                    {
                        "Image",
                        $"Le format {FileManager.GetExtension(fl)} n'est pas pris en charge pour un fichier de texture !"
                    }
                }
            };
        return await FileManager.CreateFile(fl, $"{Name}_{fl.Name}", _env, new[] { Tools.Locations.Texture });
    }
}