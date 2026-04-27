using MediatR;

namespace Catalog.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsAvailable { get; set; }
    public Guid CategoryId { get; set; }
}
