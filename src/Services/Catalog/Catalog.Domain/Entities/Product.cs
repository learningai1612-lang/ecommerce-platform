using Catalog.Domain.Common;

namespace Catalog.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Sku { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public Guid CategoryId { get; set; }

    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}
