using FluentValidation;

namespace Cart.Application.Carts.Commands.ClearCart;

public class ClearCartCommandValidator : AbstractValidator<ClearCartCommand>
{
    public ClearCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("User ID must be a valid GUID.");
    }
}
