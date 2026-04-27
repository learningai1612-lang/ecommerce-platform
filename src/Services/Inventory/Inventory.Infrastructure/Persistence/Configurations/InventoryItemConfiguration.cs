using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.ProductId)
            .IsRequired();

        builder.HasIndex(i => i.ProductId)
            .IsUnique()
            .HasDatabaseName("IX_InventoryItems_ProductId");

        builder.Property(i => i.AvailableQuantity)
            .IsRequired();

        builder.Property(i => i.ReservedQuantity)
            .IsRequired();

        builder.Property(i => i.ReorderLevel)
            .IsRequired();

        // Optimistic concurrency control
        builder.Property(i => i.RowVersion)
            .IsRowVersion()
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .IsRequired();

        // Computed property - not mapped to database
        builder.Ignore(i => i.TotalQuantity);
        builder.Ignore(i => i.IsLowStock);
    }
}
