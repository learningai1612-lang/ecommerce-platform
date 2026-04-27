using MediatR;

namespace Cart.Application.Carts.Commands.RemoveItemFromCart;

public class RemoveItemFromCartCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public Guid ItemId { get; set; }
}
