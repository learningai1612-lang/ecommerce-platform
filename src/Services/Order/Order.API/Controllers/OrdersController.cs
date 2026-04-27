using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Orders.Commands.CreateOrder;
using Order.Application.Orders.Commands.UpdateOrderStatus;
using Order.Application.Orders.Queries.GetOrderById;
using Order.Application.Orders.Queries.GetOrdersByUser;
using Order.Domain.Enums;

namespace Order.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateOrder([FromBody] CreateOrderCommand command)
    {
        _logger.LogInformation(
            "API: Received create order request for user {UserId} with {ItemCount} item(s)",
            command.UserId,
            command.Items?.Count ?? 0);
        var orderId = await _mediator.Send(command);
        _logger.LogInformation("API: Order {OrderId} created successfully", orderId);
        return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, orderId);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        _logger.LogInformation("API: Retrieving order {OrderId}", id);
        var query = new GetOrderByIdQuery { OrderId = id };
        var order = await _mediator.Send(query);
        return Ok(order);
    }

    /// <summary>
    /// Get orders by user ID with optional status filter
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByUser(
        Guid userId,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation(
            "API: Retrieving orders for user {UserId}. Status: {Status}, Page: {PageNumber}, Size: {PageSize}",
            userId,
            status?.ToString() ?? "All",
            pageNumber,
            pageSize);

        var query = new GetOrdersByUserQuery
        {
            UserId = userId,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var orders = await _mediator.Send(query);
        return Ok(orders);
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request)
    {
        _logger.LogInformation(
            "API: Updating order {OrderId} status to {NewStatus}",
            id,
            request.NewStatus);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = id,
            NewStatus = request.NewStatus,
            Reason = request.Reason
        };

        await _mediator.Send(command);
        return NoContent();
    }
}

public record UpdateOrderStatusRequest(OrderStatus NewStatus, string? Reason);
