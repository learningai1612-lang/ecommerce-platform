using MediatR;

namespace Inventory.Application.Inventory.Commands.AddStock;

public class AddStockCommand : IRequest<Guid>
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public int? ReorderLevel { get; set; }
}
