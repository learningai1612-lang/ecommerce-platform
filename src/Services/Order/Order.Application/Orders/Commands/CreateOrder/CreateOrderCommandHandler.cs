using MediatR;
using Microsoft.Extensions.Logging;
using Order.Application.Common.Interfaces;
using Order.Domain.Entities;
using Order.Domain.Enums;

namespace Order.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderDbContext _context;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderDbContext context,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var totalQuantity = request.Items.Sum(i => i.Quantity);
        var estimatedTotal = request.Items.Sum(i => i.UnitPrice * i.Quantity);

        _logger.LogInformation(
            "Creating order for user {UserId} with {ItemCount} items. Total Quantity: {TotalQuantity}, Estimated Total: {EstimatedTotal:C}, Shipping: {ShippingAddress}",
            request.UserId,
            request.Items.Count,
            totalQuantity,
            estimatedTotal,
            request.ShippingAddress.Length > 50 ? request.ShippingAddress.Substring(0, 50) + "..." : request.ShippingAddress);

        // Create order items from DTOs
        var orderItems = request.Items.Select(item => new OrderItem(
            item.ProductId,
            item.ProductName,
            item.UnitPrice,
            item.Quantity
        )).ToList();

        // Create the order aggregate
        var order = new OrderAggregate(
            request.UserId,
            request.ShippingAddress,
            orderItems,
            request.Notes
        );

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order {OrderId} created successfully for user {UserId}. Status: {Status}, Total Amount: {TotalAmount:C}, Item Count: {ItemCount}",
            order.Id,
            order.UserId,
            order.Status,
            order.TotalAmount,
            order.Items.Count);

        // TODO: Publish domain events to message broker for event-driven architecture
        // This would integrate with RabbitMQ, Azure Service Bus, or other message brokers
        // to enable saga orchestration and microservice communication
        var domainEvents = order.DomainEvents;
        if (domainEvents.Count > 0)
        {
            _logger.LogInformation(
                "Order {OrderId} has {EventCount} domain event(s) ready for publishing: {EventTypes}",
                order.Id,
                domainEvents.Count,
                string.Join(", ", domainEvents.Select(e => e.GetType().Name)));
        }

        order.ClearDomainEvents();

        return order.Id;
    }
}
