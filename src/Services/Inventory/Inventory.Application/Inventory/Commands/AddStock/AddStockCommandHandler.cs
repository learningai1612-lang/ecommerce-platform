using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Inventory.Commands.AddStock;

public class AddStockCommandHandler : IRequestHandler<AddStockCommand, Guid>
{
    private readonly IInventoryDbContext _context;
    private readonly ILogger<AddStockCommandHandler> _logger;

    public AddStockCommandHandler(
        IInventoryDbContext context,
        ILogger<AddStockCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Handle(AddStockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adding stock for Product {ProductId}. Quantity: {Quantity}, ReorderLevel: {ReorderLevel}",
            request.ProductId, request.Quantity, request.ReorderLevel);

        var inventory = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId, cancellationToken);

        if (inventory == null)
        {
            // Create new inventory item
            inventory = new InventoryItem(
                request.ProductId,
                request.Quantity,
                request.ReorderLevel ?? 10); // Default reorder level

            _context.InventoryItems.Add(inventory);

            _logger.LogInformation(
                "Created new inventory for Product {ProductId}. Initial Quantity: {Quantity}, Reorder Level: {ReorderLevel}",
                request.ProductId, request.Quantity, inventory.ReorderLevel);
        }
        else
        {
            // Add to existing inventory
            var previousQuantity = inventory.AvailableQuantity;
            inventory.AddStock(request.Quantity);

            if (request.ReorderLevel.HasValue)
            {
                inventory.UpdateReorderLevel(request.ReorderLevel.Value);
            }

            _logger.LogInformation(
                "Added stock to Product {ProductId}. Previous: {Previous}, Added: {Added}, New Total: {NewTotal}",
                request.ProductId, previousQuantity, request.Quantity, inventory.AvailableQuantity);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Stock update completed for Product {ProductId}. Inventory ID: {InventoryId}",
            request.ProductId, inventory.Id);

        return inventory.Id;
    }
}
