using MediatR;

namespace Cart.Application.Carts.Commands.UpdateCartItemQuantity;

public class UpdateCartItemQuantityCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
}
