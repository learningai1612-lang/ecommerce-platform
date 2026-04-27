using Catalog.Application.Common.Exceptions;
using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly ICatalogDbContext _context;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(ICatalogDbContext context, ILogger<UpdateProductCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating product - Id: {ProductId}, Name: {ProductName}", request.Id, request.Name);

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Failed to update product - Product not found: {ProductId}", request.Id);
            throw new NotFoundException(nameof(Product), request.Id);
        }

        // Verify category exists
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            _logger.LogWarning("Failed to update product '{ProductName}' - Category not found: {CategoryId}", 
                product.Name, request.CategoryId);
            throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        var oldPrice = product.Price;
        var oldStock = product.Stock;
        
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.IsAvailable = request.IsAvailable;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product updated successfully - Id: {ProductId}, Name: {ProductName}, Price: {OldPrice} -> {NewPrice}, Stock: {OldStock} -> {NewStock}", 
            product.Id, product.Name, oldPrice, product.Price, oldStock, product.Stock);

        return Unit.Value;
    }
}
