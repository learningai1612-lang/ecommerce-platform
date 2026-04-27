using Order.Domain.Enums;

namespace Order.Application.Common.DTOs;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
