using MediatR;
using Order.Domain.Enums;

namespace Order.Application.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommand : IRequest<Unit>
{
    public Guid OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
    public string? Reason { get; set; }
}
