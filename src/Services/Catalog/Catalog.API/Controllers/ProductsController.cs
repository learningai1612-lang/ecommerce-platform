using Catalog.Application.Products.Commands.CreateProduct;
using Catalog.Application.Products.Commands.UpdateProduct;
using Catalog.Application.Products.Queries.GetProductById;
using Catalog.Application.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all products with pagination and filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query)
    {
        _logger.LogInformation("Received request: GET /api/products - Page: {PageNumber}, Size: {PageSize}, Filters: Category={CategoryId}, Search={SearchTerm}", 
            query.PageNumber, query.PageSize, query.CategoryId, query.SearchTerm);

        var result = await _mediator.Send(query);
        
        _logger.LogDebug("Returning {Count} products from {Total} total", result.Items.Count, result.TotalCount);
        return Ok(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        _logger.LogInformation("Received request: GET /api/products/{ProductId}", id);

        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query);
        
        _logger.LogDebug("Product retrieved: {ProductName}", result.Name);
        return Ok(result);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        _logger.LogInformation("Received request: POST /api/products - Name: {ProductName}, SKU: {Sku}", command.Name, command.Sku);

        var productId = await _mediator.Send(command);
        
        _logger.LogInformation("Product created successfully with ID: {ProductId}", productId);
        return CreatedAtAction(nameof(GetProduct), new { id = productId }, new { id = productId });
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
    {
        _logger.LogInformation("Received request: PUT /api/products/{ProductId}", id);

        if (id != command.Id)
        {
            _logger.LogWarning("Product ID mismatch - URL: {UrlId}, Body: {BodyId}", id, command.Id);
            return BadRequest(new { error = "Product ID mismatch between URL and request body" });
        }

        await _mediator.Send(command);
        
        _logger.LogInformation("Product updated successfully: {ProductId}", id);
        return NoContent();
    }
}
