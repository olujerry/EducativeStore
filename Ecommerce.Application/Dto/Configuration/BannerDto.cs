using Ecommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.Dto;
public class BannerDto
{
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public string? ColorHexCode { get; set; }
    public string? BackgroundColorHexCode { get; set; }
    public string? BannerType { get; set; }
    public bool IsActive { get; set; }
}