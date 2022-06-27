using System.Text.Json.Serialization;
using ApiVrEdu.Models.Elements;
using ApiVrEdu.Models.Reactions;
using ApiVrEdu.Models.Textures;
using Microsoft.AspNetCore.Identity;

namespace ApiVrEdu.Models;

public enum SexType
{
    H,
    F
}

public class User : IdentityUser<int>, IModelImage
{
    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }
    public bool IsActivated { get; set; }

    public string LastName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    public SexType Sex { get; set; }

    [JsonIgnore] public override string PasswordHash { get; set; } = string.Empty;


    public virtual List<ElementGroup> ElementGroups { get; set; } = new();
    public virtual List<Element> Elements { get; set; } = new();
    public virtual List<ElementType> ElementTypes { get; set; } = new();
    public virtual List<TextureGroup> TextureGroups { get; set; } = new();
    public virtual List<Texture> Textures { get; set; } = new();
    public virtual List<Reaction> Reactions { get; set; } = new();
    public string? Image { get; set; } = string.Empty;
}