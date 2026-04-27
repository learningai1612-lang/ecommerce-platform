using MediatR;

namespace Catalog.Application.Products.Commands.CreateProduct;

public class CreateProductCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Sku { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public List<string>? ImageUrls { get; set; }
}
