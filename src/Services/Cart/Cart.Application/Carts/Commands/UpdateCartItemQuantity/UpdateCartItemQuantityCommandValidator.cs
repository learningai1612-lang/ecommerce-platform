using FluentValidation;

namespace Cart.Application.Carts.Commands.UpdateCartItemQuantity;

public class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("User ID must be a valid GUID.");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Item ID must be a valid GUID.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(999).WithMessage("Quantity must not exceed 999.");
    }
}
