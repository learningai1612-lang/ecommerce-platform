using Microsoft.EntityFrameworkCore;
using Order.Application.Common.Interfaces;
using Order.Domain.Entities;

namespace Order.Infrastructure.Persistence;

public class OrderDbContext : DbContext, IOrderDbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrderAggregate> Orders => Set<OrderAggregate>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
