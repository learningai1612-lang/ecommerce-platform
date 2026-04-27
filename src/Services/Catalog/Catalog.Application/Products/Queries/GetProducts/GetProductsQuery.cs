using Catalog.Application.Common.Models;
using Catalog.Application.Products.Queries.DTOs;
using MediatR;

namespace Catalog.Application.Products.Queries.GetProducts;

public class GetProductsQuery : IRequest<PaginatedList<ProductDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsAvailable { get; set; }
}
