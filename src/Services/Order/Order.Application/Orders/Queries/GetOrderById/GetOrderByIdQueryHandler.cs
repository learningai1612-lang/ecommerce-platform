using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Application.Common.DTOs;
using Order.Application.Common.Exceptions;
using Order.Application.Common.Interfaces;
using Order.Domain.Entities;

namespace Order.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponseDto>
{
    private readonly IOrderDbContext _context;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(
        IOrderDbContext context,
        ILogger<GetOrderByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving order {OrderId}", request.OrderId);

        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", request.OrderId);
            throw new NotFoundException(nameof(OrderAggregate), request.OrderId);
        }

        var response = new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(item => new OrderItemResponseDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                SubTotal = item.Subtotal
            }).ToList()
        };

        _logger.LogInformation(
            "Order {OrderId} retrieved successfully. Status: {Status}, Items: {ItemCount}, Total: {TotalAmount:C}",
            order.Id,
            order.Status,
            order.Items.Count,
            order.TotalAmount);

        return response;
    }
}
