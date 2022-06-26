using System.ComponentModel.DataAnnotations;
using ApiVrEdu.Models;

namespace ApiVrEdu.Dto;

public class UpdateUserAdminDto
{
    [Required] public int Id { get; set; }

    public string? UserName { get; set; }

    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }

    public string? Lastname { get; set; }

    public string? Firstname { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public IFormFile? Image { get; set; }

    public DateTime? BirthDate { get; set; }

    public SexType? Sex { get; set; }
    public bool IsActivated { get; set; }
}