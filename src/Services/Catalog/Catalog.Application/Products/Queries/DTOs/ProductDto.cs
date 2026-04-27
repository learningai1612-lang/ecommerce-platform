namespace Catalog.Application.Products.Queries.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Sku { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<ProductImageDto> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProductImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}
