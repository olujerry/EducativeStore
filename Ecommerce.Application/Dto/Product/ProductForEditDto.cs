using System.Text.Json;

namespace Ecommerce.Application.Dto;

public class ProductForEditDto
{
    public long ProductId { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? KeySpecs { get; set; }
    public List<string> KeySpecsList => JsonSerializer.Deserialize<List<string>>(KeySpecs ?? "[]");
    public string? Description { get; set; }
    public string? VariableTheme { get; set; }
    public int CategoryId { get; set; }
    public string? ProductImage { get; set; }
    public string? ProductImagePreview { get; set; }
    public List<ProductVariantForEditDto> ProductVariant { get; set; } = new List<ProductVariantForEditDto>();
}

public class ProductVariantForEditDto
{
    public long Id { get; set; }
    public string? CategoryName { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public long ProductId { get; set; }
    public int? SizeId { get; set; }
    public int? ColorId { get; set; }
    public string? VariantImageId { get; set; }
    public string? VariantImagePreview { get; set; }
    public string? Sku { get; set; }
    public int Qty { get; set; }
    public decimal Price { get; set; }
}
