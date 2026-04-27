using Catalog.Application.Common.Interfaces;
using Catalog.Application.Common.Models;
using Catalog.Application.Products.Queries.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedList<ProductDto>>
{
    private readonly ICatalogDbContext _context;
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public GetProductsQueryHandler(ICatalogDbContext context, ILogger<GetProductsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching products - Page: {PageNumber}/{PageSize}, Category: {CategoryId}, PriceRange: {MinPrice}-{MaxPrice}, Search: {SearchTerm}, Available: {IsAvailable}", 
            request.PageNumber, request.PageSize, request.CategoryId, request.MinPrice, request.MaxPrice, request.SearchTerm ?? "None", request.IsAvailable);

        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsQueryable();

        // Apply filters
        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchTerm) || 
                                    p.Description.ToLower().Contains(searchTerm) ||
                                    p.Sku.ToLower().Contains(searchTerm));
        }

        if (request.IsAvailable.HasValue)
        {
            query = query.Where(p => p.IsAvailable == request.IsAvailable.Value);
        }

        // Order by created date descending
        query = query.OrderByDescending(p => p.CreatedAt);

        // Project to DTO
        var dtoQuery = query.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            Sku = p.Sku,
            IsAvailable = p.IsAvailable,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.Name,
            Images = p.Images.OrderBy(i => i.DisplayOrder).Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                AltText = i.AltText,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder
            }).ToList(),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });

        var result = await PaginatedList<ProductDto>.CreateAsync(dtoQuery, request.PageNumber, request.PageSize);

        _logger.LogInformation("Products fetched successfully - Returned: {ItemCount}, Total: {TotalCount}, Page: {PageNumber}/{TotalPages}", 
            result.Items.Count, result.TotalCount, result.PageNumber, result.TotalPages);

        return result;
    }
}
