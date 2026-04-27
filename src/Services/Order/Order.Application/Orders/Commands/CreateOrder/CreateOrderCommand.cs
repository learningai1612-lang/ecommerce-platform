using MediatR;
using Order.Application.Common.DTOs;

namespace Order.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<Guid>
{
    public Guid UserId { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
