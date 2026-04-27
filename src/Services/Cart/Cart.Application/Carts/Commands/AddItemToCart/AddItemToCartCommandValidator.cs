using FluentValidation;

namespace Cart.Application.Carts.Commands.AddItemToCart;

public class AddItemToCartCommandValidator : AbstractValidator<AddItemToCartCommand>
{
    public AddItemToCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("User ID must be a valid GUID.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Product ID must be a valid GUID.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MinimumLength(2).WithMessage("Product name must be at least 2 characters.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.")
            .LessThanOrEqualTo(1000000).WithMessage("Price must not exceed 1,000,000.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Quantity must not exceed 100 per addition.");
    }
}
