using Catalog.Application.Common.Exceptions;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Products.Queries.DTOs;
using Catalog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly ICatalogDbContext _context;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;

    public GetProductByIdQueryHandler(ICatalogDbContext context, ILogger<GetProductByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching product: {ProductId}", request.Id);

        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", request.Id);
            throw new NotFoundException(nameof(Product), request.Id);
        }

        _logger.LogInformation("Product retrieved successfully - Id: {ProductId}, Name: {ProductName}, SKU: {Sku}", 
            product.Id, product.Name, product.Sku);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            Sku = product.Sku,
            IsAvailable = product.IsAvailable,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            Images = product.Images.OrderBy(i => i.DisplayOrder).Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                AltText = i.AltText,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder
            }).ToList(),
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
