using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ApiVrEdu.Models.Elements;
using ApiVrEdu.Models.Textures;

namespace ApiVrEdu.Models;

public enum SexType
{
    H,
    F
}

public class User : BaseModel, IModelImage
{
    [Index(IsUnique = true)]
    [Required(ErrorMessage = "Nom d'utilisateur obligatoire")]
    public string UserName { get; set; } = string.Empty;

    [JsonIgnore]
    [Required(ErrorMessage = "Mot de passe obligatoire")]
    public string HashedPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nom obligatoire")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Prenom obligatoire")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date de naissance obligatoire")]
    public DateTime BirthDate { get; set; }

    [Required(ErrorMessage = "Numero de telephone obligatoire")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Index(IsUnique = true)]
    [Required(ErrorMessage = "Email obligatoire")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sexe obligatoire")]
    public SexType Sex { get; set; }

    public bool IsAdmin { get; set; } = false;

    [InverseProperty("User")] public virtual List<ElementGroup> ElementGroups { get; set; } = new();
    [InverseProperty("User")] public virtual List<Element> Elements { get; set; } = new();
    [InverseProperty("User")] public virtual List<ElementType> ElementTypes { get; set; } = new();
    [InverseProperty("User")] public virtual List<Reaction> Reactions { get; set; } = new();
    [InverseProperty("User")] public virtual List<TextureGroup> TextureGroups { get; set; } = new();
    [InverseProperty("User")] public virtual List<Texture> Textures { get; set; } = new();
    public string? Image { get; set; } = string.Empty;
}