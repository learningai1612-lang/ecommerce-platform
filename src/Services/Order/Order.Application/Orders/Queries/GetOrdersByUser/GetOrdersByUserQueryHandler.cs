using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Application.Common.DTOs;
using Order.Application.Common.Interfaces;
using Order.Application.Common.Models;

namespace Order.Application.Orders.Queries.GetOrdersByUser;

public class GetOrdersByUserQueryHandler : IRequestHandler<GetOrdersByUserQuery, PaginatedList<OrderResponseDto>>
{
    private readonly IOrderDbContext _context;
    private readonly ILogger<GetOrdersByUserQueryHandler> _logger;

    public GetOrdersByUserQueryHandler(
        IOrderDbContext context,
        ILogger<GetOrdersByUserQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<OrderResponseDto>> Handle(GetOrdersByUserQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving orders for user {UserId}. Status Filter: {StatusFilter}, Page: {PageNumber}, Size: {PageSize}",
            request.UserId,
            request.Status?.ToString() ?? "All",
            request.PageNumber,
            request.PageSize);

        var query = _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == request.UserId);

        if (request.Status.HasValue)
        {
            query = query.Where(o => o.Status == request.Status.Value);
        }

        query = query.OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var orderDtos = orders.Select(order => new OrderResponseDto
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
        }).ToList();

        _logger.LogInformation(
            "Retrieved {Count} orders for user {UserId} (Total: {TotalCount}, Page {PageNumber}/{TotalPages})",
            orderDtos.Count,
            request.UserId,
            totalCount,
            request.PageNumber,
            (int)Math.Ceiling(totalCount / (double)request.PageSize));

        return new PaginatedList<OrderResponseDto>(orderDtos, totalCount, request.PageNumber, request.PageSize);
    }
}
