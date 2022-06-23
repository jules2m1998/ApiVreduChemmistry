using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApiVrEdu.Models;

public enum SexType
{
    H,
    F
}

public class User : BaseModel
{
    [InverseProperty("User")] public List<Element> Elements = new();

    [InverseProperty("User")] public List<Reaction> Reactions = new();
    public string UserName { get; set; } = string.Empty;

    [JsonIgnore] public string HashedPassword { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? Image { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public SexType Sex { get; set; }
    public bool IsAdmin { get; set; } = false;
}