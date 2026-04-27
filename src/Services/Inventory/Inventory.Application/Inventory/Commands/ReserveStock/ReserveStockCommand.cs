using MediatR;

namespace Inventory.Application.Inventory.Commands.ReserveStock;

public class ReserveStockCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
