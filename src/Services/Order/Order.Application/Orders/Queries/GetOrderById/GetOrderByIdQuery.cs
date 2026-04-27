using MediatR;
using Order.Application.Common.DTOs;

namespace Order.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdQuery : IRequest<OrderResponseDto>
{
    public Guid OrderId { get; set; }
}
