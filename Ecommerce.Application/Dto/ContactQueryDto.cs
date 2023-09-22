using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.Dto;

public class ContactQueryDto
{
    public long Id { get; set; }
    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; }
    [Required]
    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Invalid Email Address!")]
    public string Email { get; set; }
    [Required]
    [StringLength(50, ErrorMessage = "Max 50 Character are Allowed for Subject!")]
    public string Subject { get; set; }
    [Required]
    [Display(Name = "Message Body")]
    [StringLength(200, ErrorMessage = "Max 200 Character are Allowed for Message Body!")]
    public string MessageBody { get; set; }
}
