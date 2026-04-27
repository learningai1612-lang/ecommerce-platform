using FluentValidation;
using Order.Domain.Enums;

namespace Order.Application.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Order ID must be a valid GUID.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Order status must be a valid value (Pending, PaymentPending, Paid, Failed, Cancelled, Shipped, or Completed).");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .When(x => x.NewStatus == OrderStatus.Failed || x.NewStatus == OrderStatus.Cancelled)
            .WithMessage("Reason is required when marking order as Failed or Cancelled.")
            .MinimumLength(5)
            .When(x => !string.IsNullOrEmpty(x.Reason))
            .WithMessage("Reason must be at least 5 characters when provided.")
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters.");
    }
}
