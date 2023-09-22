using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.Dto;

public class AddEditRoleDto
{
    public string Id { get; set; }
    [Required]
    public string Name { get; set; }
}
