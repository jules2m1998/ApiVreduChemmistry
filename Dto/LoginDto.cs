using System.ComponentModel.DataAnnotations;

namespace ApiVrEdu.Dto;

public class LoginDto
{
    [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire !")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire !")]
    public string Password { get; set; } = string.Empty;
}