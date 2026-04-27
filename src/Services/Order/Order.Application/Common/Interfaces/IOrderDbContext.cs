using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;

namespace Order.Application.Common.Interfaces;

public interface IOrderDbContext
{
    DbSet<OrderAggregate> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
