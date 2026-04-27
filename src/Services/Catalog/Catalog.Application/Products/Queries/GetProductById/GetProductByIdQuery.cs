using Catalog.Application.Products.Queries.DTOs;
using MediatR;

namespace Catalog.Application.Products.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<ProductDto>
{
    public Guid Id { get; set; }

    public GetProductByIdQuery(Guid id)
    {
        Id = id;
    }
}
