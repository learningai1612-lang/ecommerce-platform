using MediatR;
using Order.Application.Common.DTOs;
using Order.Application.Common.Models;
using Order.Domain.Enums;

namespace Order.Application.Orders.Queries.GetOrdersByUser;

public class GetOrdersByUserQuery : IRequest<PaginatedList<OrderResponseDto>>
{
    public Guid UserId { get; set; }
    public OrderStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
