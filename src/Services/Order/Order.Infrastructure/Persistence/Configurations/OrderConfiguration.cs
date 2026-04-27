using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;
using Order.Domain.Enums;

namespace Order.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<OrderAggregate>
{
    public void Configure(EntityTypeBuilder<OrderAggregate> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.UserId)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasConversion(
                v => v.ToString(),
                v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(o => o.ShippingAddress)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(o => o.Notes)
            .HasMaxLength(1000);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .IsRequired();

        // Configure the Items collection as a one-to-many relationship
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events - they are transient
        builder.Ignore(o => o.DomainEvents);

        // Add index for user queries
        builder.HasIndex(o => o.UserId);

        // Add index for status queries
        builder.HasIndex(o => o.Status);

        // Composite index for user + status queries
        builder.HasIndex(o => new { o.UserId, o.Status });
    }
}
