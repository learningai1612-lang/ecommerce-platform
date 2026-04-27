using Inventory.Application.Inventory.Commands.AddStock;
using Inventory.Application.Inventory.Commands.ReleaseStock;
using Inventory.Application.Inventory.Commands.ReserveStock;
using Inventory.Application.Inventory.Queries.GetInventoryByProductId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IMediator mediator, ILogger<InventoryController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(InventoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryResponseDto>> GetInventory(Guid productId)
    {
        var query = new GetInventoryByProductIdQuery { ProductId = productId };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { error = $"Inventory not found for product {productId}" });
        }

        return Ok(result);
    }

    [HttpPost("add")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> AddStock([FromBody] AddStockCommand command)
    {
        _logger.LogInformation("Received request to add stock for Product {ProductId}", command.ProductId);
        
        var inventoryId = await _mediator.Send(command);
        return Ok(new { inventoryId, message = "Stock added successfully" });
    }

    [HttpPost("reserve")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> ReserveStock([FromBody] ReserveStockCommand command)
    {
        _logger.LogInformation(
            "Received request to reserve stock for Order {OrderId}, Product {ProductId}",
            command.OrderId, command.ProductId);
        
        var success = await _mediator.Send(command);
        
        if (!success)
        {
            return BadRequest(new { error = "Failed to reserve stock. Insufficient quantity or product not found." });
        }

        return Ok(new { success = true, message = "Stock reserved successfully" });
    }

    [HttpPost("release")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> ReleaseStock([FromBody] ReleaseStockCommand command)
    {
        _logger.LogInformation(
            "Received request to release stock for Order {OrderId}, Product {ProductId}",
            command.OrderId, command.ProductId);
        
        var success = await _mediator.Send(command);
        
        if (!success)
        {
            return BadRequest(new { error = "Failed to release stock. Invalid quantity or product not found." });
        }

        return Ok(new { success = true, message = "Stock released successfully" });
    }
}
