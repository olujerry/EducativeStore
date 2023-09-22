using Ecommerce.Application.Dto;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Dto;

public class OrderInfoDetailsDto
{
    public List<OrderItemDetailsDto> OrderItemDetails { get; set; }
    public UpdateOrderStatusDto UpdateOrderStatus { get; set; }
    public Order? Order { get; set; }
    public List<OrderStatus>? OrderStatus { get; set; }
    public OrderStatus? CurrentOrderStatus { get; set; }
    public CustomerDto? CustomerInfo { get; set; }
}
public class OrderItemDetailsDto
{
    public long ProductId { get; set; }
    public string ProductSlug { get; set; }
    public long ProductVariantId { get; set; }
    public string? OrderItemTitle { get; set; }
    public string? OrderItemImage { get; set; }
    public decimal ItemUnitPrice { get; set; }
    public int ItemQty { get; set; }
}
public class UpdateOrderStatusDto
{
    public long OrderId { get; set; }
    public string? InvoiceNo { get; set; }
    public string? CurrentOrderStatus { get; set; }
    public int NewOrderStatus { get; set; }
    public string? Description { get; set; }
    public DateTime? CurrentOrderStatusTime { get; set; }
}

public class UpdateOrderShippingInfoDto
{
    public long OrderId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerMobile { get; set; }
    public string CustomerEmail { get; set; }
    public string ShippingAddress { get; set; }
    public string? CustomerComment { get; set; }
}
