using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.Dto;

public class AddEditUserDto
{
    public string? Id { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    public string? FullName { get; set; }
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }
    public string? ProfilePicturePreview { get; set; }
    public string? ProfilePicture { get; set; }
    [Required]
    public string? Gender { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefaultPassword { get; set; }
    public bool EmailConfirmed { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }
}
