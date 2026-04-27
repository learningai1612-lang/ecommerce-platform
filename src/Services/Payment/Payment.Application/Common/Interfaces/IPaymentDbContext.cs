using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;

namespace Payment.Application.Common.Interfaces;

public interface IPaymentDbContext
{
    DbSet<PaymentTransaction> Payments { get; }
    DbSet<ProcessedEvent> ProcessedEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
