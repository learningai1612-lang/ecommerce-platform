using Inventory.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Inventory.Queries.GetInventoryByProductId;

public class GetInventoryByProductIdQueryHandler : IRequestHandler<GetInventoryByProductIdQuery, InventoryResponseDto?>
{
    private readonly IInventoryDbContext _context;
    private readonly ILogger<GetInventoryByProductIdQueryHandler> _logger;

    public GetInventoryByProductIdQueryHandler(
        IInventoryDbContext context,
        ILogger<GetInventoryByProductIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<InventoryResponseDto?> Handle(
        GetInventoryByProductIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving inventory for Product {ProductId}", request.ProductId);

        var inventory = await _context.InventoryItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId, cancellationToken);

        if (inventory == null)
        {
            _logger.LogWarning("Inventory not found for Product {ProductId}", request.ProductId);
            return null;
        }

        return new InventoryResponseDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            AvailableQuantity = inventory.AvailableQuantity,
            ReservedQuantity = inventory.ReservedQuantity,
            TotalQuantity = inventory.TotalQuantity,
            ReorderLevel = inventory.ReorderLevel,
            IsLowStock = inventory.IsLowStock,
            UpdatedAt = inventory.UpdatedAt
        };
    }
}
