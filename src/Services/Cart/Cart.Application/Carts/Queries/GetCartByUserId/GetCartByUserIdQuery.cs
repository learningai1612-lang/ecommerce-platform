using MediatR;
using Cart.Application.Common.DTOs;

namespace Cart.Application.Carts.Queries.GetCartByUserId;

public class GetCartByUserIdQuery : IRequest<CartResponseDto?>
{
    public Guid UserId { get; set; }
}
