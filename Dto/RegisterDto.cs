using System.ComponentModel.DataAnnotations;
using ApiVrEdu.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiVrEdu.Dto;

public class RegisterDto
{
    [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire !")]
    [FromForm]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire !")]
    [FromForm]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire !")]
    [FromForm]
    public string Lastname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prenom est obligatoire !")]
    [FromForm]
    public string Firstname { get; set; } = string.Empty;

    [Required]
    [FromForm]
    [EmailAddress(ErrorMessage = "L'adresse email n'est pas valid")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [FromForm]
    [Phone(ErrorMessage = "Le numero de telephone n'est pas valid")]
    public string PhoneNumber { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    [Required(ErrorMessage = "La date de naissance est obligatoire !")]
    [FromForm]
    public DateTime BirthDate { get; set; }

    [Required(ErrorMessage = "Le sexe est obligatoire !")]
    [FromForm]
    public SexType Sex { get; set; }
}