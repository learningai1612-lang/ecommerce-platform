using Catalog.Application.Common.Exceptions;
using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly ICatalogDbContext _context;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(ICatalogDbContext context, ILogger<CreateProductCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product - Name: {ProductName}, SKU: {Sku}, Price: {Price}, CategoryId: {CategoryId}", 
            request.Name, request.Sku, request.Price, request.CategoryId);

        // Verify category exists
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            _logger.LogWarning("Failed to create product '{ProductName}' - Category not found: {CategoryId}", 
                request.Name, request.CategoryId);
            throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Sku = request.Sku,
            CategoryId = request.CategoryId,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add images if provided
        if (request.ImageUrls != null && request.ImageUrls.Any())
        {
            var images = request.ImageUrls.Select((url, index) => new ProductImage
            {
                Id = Guid.NewGuid(),
                ImageUrl = url,
                AltText = request.Name,
                IsPrimary = index == 0,
                DisplayOrder = index,
                ProductId = product.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            product.Images = images;
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created successfully - Id: {ProductId}, Name: {ProductName}, SKU: {Sku}", 
            product.Id, product.Name, product.Sku);

        // TODO: Publish ProductCreatedEvent for event-driven architecture
        // var productCreatedEvent = new ProductCreatedEvent
        // {
        //     ProductId = product.Id,
        //     ProductName = product.Name,
        //     Price = product.Price,
        //     CreatedAt = product.CreatedAt
        // };

        return product.Id;
    }
}
