using ApiVrEdu.Exceptions;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models.Textures;

namespace ApiVrEdu.Dto.Textures;

public class TextureUpdateDto
{
    private IWebHostEnvironment _env;
    public string? Name { get; set; }
    public double? DisplacementScale { get; set; }
    public double? Roughness { get; set; }
    public double? Metalness { get; set; }
    public int? R { get; set; }
    public int? G { get; set; }
    public int? B { get; set; }
    public float? A { get; set; }

    public IFormFile? RoughnessMap { get; set; }
    public IFormFile? Map { get; set; }
    public IFormFile? NormalMap { get; set; }
    public IFormFile? DisplacementMap { get; set; }
    public IFormFile? AoMap { get; set; }
    public IFormFile? MetalnessMap { get; set; }
    public TextureType? TextureType { get; set; }

    public async Task<Texture> ToTexture(Texture texture, IWebHostEnvironment env)
    {
        _env = env;
        if (Name is not null) texture.Name = Name;
        if (DisplacementScale is not null) texture.DisplacementScale = (double)DisplacementScale;
        if (Roughness is not null) texture.Roughness = (double)Roughness;
        if (Metalness is not null) texture.Metalness = (double)Metalness;
        if (R is not null) texture.Color.R = (int)R;
        if (G is not null) texture.Color.G = (int)G;
        if (B is not null) texture.Color.B = (int)B;
        if (A is not null) texture.Color.A = (float)A;

        if (TextureType is not null) texture.TextureType = (TextureType)TextureType;

        await Tools.LoopToUpdateFile(texture, this, env, new[] { "" });


        return texture;
    }

    private async Task<string?> FileToString(IFormFile? file)
    {
        if (file is null) return null;
        string[] acceptExits = { "png", "jpg", "jpeg" };
        if (!acceptExits.Contains(FileManager.GetExtension(file)))
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
        return await FileManager.CreateFile(file, $"{Name}_{file.Name}", _env, new[] { Tools.Locations.Texture });
    }
}