using System.ComponentModel.DataAnnotations;
using ApiVrEdu.Helpers;
using ApiVrEdu.Models.Elements;

namespace ApiVrEdu.Dto.Elements;

public class UpdateElementDto
{
    [Required(ErrorMessage = "Identifiant obligatoire !")]
    public int Id { get; set; }

    public string? Name { get; set; } = string.Empty;

    public string? Symbol { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    public int? MassNumber { get; set; }

    public int? AtomicNumber { get; set; }

    public int? GroupId { get; set; }

    public int? TypeId { get; set; }

    public int? TextureId { get; set; }

    public bool? IsActivated { get; set; }

    public string? Description { get; set; } = string.Empty;

    public async Task<Response?> UpdateElement(Element element, IWebHostEnvironment env)
    {
        if (Image is not null)
        {
            var ext = FileManager.GetExtension(Image);
            string[] acceptExits = { "png", "jpg", "jpeg" };
            if (!acceptExits.Contains(ext))
                return new Response
                {
                    Errors = new Dictionary<string, string>
                    {
                        { "image", $"Le format {ext} n'est pas pris en charge pour les image !" }
                    }
                };
            await Tools.LoopToUpdateFile(element, this, env, new[] { "" });
        }

        Tools.LoopToUpdateObject(element, this, new[] { "image", "GroupId", "TypeId", "TextureId" });

        return null;
    }
}