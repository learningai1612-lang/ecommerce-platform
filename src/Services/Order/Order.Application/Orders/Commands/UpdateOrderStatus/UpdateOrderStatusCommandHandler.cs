using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Application.Common.Exceptions;
using Order.Application.Common.Interfaces;
using Order.Domain.Entities;
using Order.Domain.Enums;

namespace Order.Application.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Unit>
{
    private readonly IOrderDbContext _context;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    public UpdateOrderStatusCommandHandler(
        IOrderDbContext context,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", request.OrderId);
            throw new NotFoundException(nameof(OrderAggregate), request.OrderId);
        }

        var oldStatus = order.Status;

        try
        {
            // Use the aggregate's state transition methods to enforce business rules
            switch (request.NewStatus)
            {
                case OrderStatus.Paid:
                    order.MarkAsPaid();
                    break;

                case OrderStatus.Shipped:
                    order.MarkAsShipped();
                    break;

                case OrderStatus.Completed:
                    order.MarkAsCompleted();
                    break;

                case OrderStatus.Failed:
                    if (string.IsNullOrEmpty(request.Reason))
                        throw new ValidationException();
                    order.MarkAsFailed(request.Reason);
                    break;

                case OrderStatus.Cancelled:
                    if (string.IsNullOrEmpty(request.Reason))
                        throw new ValidationException();
                    order.Cancel(request.Reason);
                    break;

                default:
                    throw new InvalidOperationException($"Status transition to {request.NewStatus} is not supported through this command.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Order {OrderId} status updated from {OldStatus} to {NewStatus}. User: {UserId}, Total: {TotalAmount:C}, Reason: {Reason}",
                order.Id,
                oldStatus,
                order.Status,
                order.UserId,
                order.TotalAmount,
                request.Reason ?? "N/A");

            // TODO: Publish domain events
            var domainEvents = order.DomainEvents;
            if (domainEvents.Count > 0)
            {
                _logger.LogInformation(
                    "Order {OrderId} status change generated {EventCount} domain event(s): {EventTypes}",
                    order.Id,
                    domainEvents.Count,
                    string.Join(", ", domainEvents.Select(e => e.GetType().Name)));
            }

            order.ClearDomainEvents();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                ex,
                "Invalid status transition for order {OrderId} from {CurrentStatus} to {NewStatus}",
                request.OrderId,
                oldStatus,
                request.NewStatus);
            throw;
        }

        return Unit.Value;
    }
}
