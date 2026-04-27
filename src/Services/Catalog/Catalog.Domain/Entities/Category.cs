using Catalog.Domain.Common;

namespace Catalog.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation property
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
