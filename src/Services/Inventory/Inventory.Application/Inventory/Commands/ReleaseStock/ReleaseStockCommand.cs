using MediatR;

namespace Inventory.Application.Inventory.Commands.ReleaseStock;

public class ReleaseStockCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}
