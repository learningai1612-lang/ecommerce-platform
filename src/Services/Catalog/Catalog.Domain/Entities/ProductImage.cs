using Catalog.Domain.Common;

namespace Catalog.Domain.Entities;

public class ProductImage : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; }
    public Guid ProductId { get; set; }

    // Navigation property
    public virtual Product Product { get; set; } = null!;
}
