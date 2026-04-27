using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Persistence.Configurations;

public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.ToTable("ProcessedEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.ProcessedAt)
            .IsRequired();

        builder.Property(e => e.Payload)
            .HasColumnType("nvarchar(max)");

        // Unique index for idempotency
        builder.HasIndex(e => e.EventId)
            .IsUnique();

        // Index for querying by event type
        builder.HasIndex(e => e.EventType);
    }
}
