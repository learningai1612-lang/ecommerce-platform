using MediatR;

namespace Cart.Application.Carts.Commands.AddItemToCart;

public class AddItemToCartCommand : IRequest<Guid>
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
