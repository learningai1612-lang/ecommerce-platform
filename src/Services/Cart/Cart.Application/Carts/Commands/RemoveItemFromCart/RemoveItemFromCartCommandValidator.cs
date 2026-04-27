using FluentValidation;

namespace Cart.Application.Carts.Commands.RemoveItemFromCart;

public class RemoveItemFromCartCommandValidator : AbstractValidator<RemoveItemFromCartCommand>
{
    public RemoveItemFromCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("User ID must be a valid GUID.");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Item ID must be a valid GUID.");
    }
}
