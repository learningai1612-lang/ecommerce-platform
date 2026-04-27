using MediatR;

namespace Cart.Application.Carts.Commands.ClearCart;

public class ClearCartCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
}
