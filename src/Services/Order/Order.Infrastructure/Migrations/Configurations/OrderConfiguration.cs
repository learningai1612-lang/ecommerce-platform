using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;
public class OrderConfiguration : IEntityTypeConfiguration<OrderAggregate>
{
    public void Configure(EntityTypeBuilder<OrderAggregate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShippingAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}