using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.OrderId)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion(
                v => v.ToString(),
                v => (PaymentStatus)Enum.Parse(typeof(PaymentStatus), v))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.TransactionId)
            .HasMaxLength(100);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(500);

        builder.Property(p => p.ProcessedAt);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // Indexes for querying
        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAt);
    }
}
