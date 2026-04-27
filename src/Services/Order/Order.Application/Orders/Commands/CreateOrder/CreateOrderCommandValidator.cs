using FluentValidation;

namespace Order.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("User ID must be a valid GUID.");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required.")
            .MinimumLength(10).WithMessage("Shipping address must be at least 10 characters.")
            .MaximumLength(500).WithMessage("Shipping address must not exceed 500 characters.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.")
            .Must(items => items != null && items.Count > 0)
            .WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Product ID is required.")
                .NotEqual(Guid.Empty).WithMessage("Product ID must be a valid GUID.");

            item.RuleFor(i => i.ProductName)
                .NotEmpty().WithMessage("Product name is required.")
                .MinimumLength(2).WithMessage("Product name must be at least 2 characters.")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThan(0).WithMessage("Unit price must be greater than 0.")
                .LessThanOrEqualTo(1000000).WithMessage("Unit price must not exceed 1,000,000.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.")
                .LessThanOrEqualTo(1000).WithMessage("Quantity must not exceed 1,000.");
        });

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.");
    }
}
