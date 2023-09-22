namespace Ecommerce.Application.Dto;
public class DealOfTheDayDto
{
    public string? Title { get; set; }
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductImagePreview { get; set; }
    public string? Category { get; set; }
    public string? ProductSlug { get; set; }
    public decimal ActualPrice { get; set; }
    public decimal OfferPrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public string? ImagePreview { get; set; }
    public bool IsActive { get; set; }
}
