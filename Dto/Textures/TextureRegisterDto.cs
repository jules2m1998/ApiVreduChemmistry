using System.ComponentModel.DataAnnotations;
using ApiVrEdu.Exceptions;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models;
using ApiVrEdu.Models.Textures;

namespace ApiVrEdu.Dto.Textures;

public class TextureRegisterDto
{
    [Required(ErrorMessage = "Nom de la texture obligatoire")]
    public string Name { get; set; } = string.Empty;

    public double? DisplacementScale { get; set; } = 1.05;
    public double? Roughness { get; set; } = 1;
    public double? Metalness { get; set; } = 0;
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

    public async Task<Texture> ToTexture(User user)
    {
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
                    A = (int)A
                }
                : new Color()
        };
        if (DisplacementScale is not null) text.DisplacementScale = (double)DisplacementScale;
        if (Roughness is not null) text.Roughness = (double)Roughness;
        if (Metalness is not null) text.Metalness = (double)Metalness;
        if (TextureType is not null) text.TextureType = (TextureType)TextureType;

        return text;
    }

    private static async Task<string?> FileToString(IFormFile? file)
    {
        if (file is null) return null;
        string[] accepteExts = { "png", "jpg", "jpeg" };
        if (!accepteExts.Contains(FileManager.GetExtension(file)))
            throw new ExceptionResponse
            {
                Errors = new Dictionary<string, string>
                {
                    {
                        "Image",
                        $"Le format {FileManager.GetExtension(file)} n'est pas pris en charge pour un fichier de texture !"
                    }
                }
            };
        return await FileManager.FileToBytes(file);
    }
}