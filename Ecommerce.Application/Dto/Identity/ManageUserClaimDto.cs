﻿using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.Dto;

public class ManageUserClaimDto
{
    [Required]
    public string UserId { get; set; }
    [Required]
    public string UserName { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
    public bool Checked { get; set; }
}
