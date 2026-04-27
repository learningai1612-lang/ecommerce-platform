using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Application.Common.Interfaces;

public interface ICatalogDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    DbSet<ProductImage> ProductImages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
