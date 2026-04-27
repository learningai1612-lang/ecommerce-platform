using MediatR;

namespace Inventory.Application.Inventory.Queries.GetInventoryByProductId;

public class GetInventoryByProductIdQuery : IRequest<InventoryResponseDto?>
{
    public Guid ProductId { get; set; }
}
