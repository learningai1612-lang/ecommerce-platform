using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Payments.Queries.GetPaymentByOrderId;

namespace Payment.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get payment by order ID
    /// </summary>
    [HttpGet("order/{orderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentByOrderId(Guid orderId)
    {
        _logger.LogInformation("API: Retrieving payment for order {OrderId}", orderId);

        var query = new GetPaymentByOrderIdQuery { OrderId = orderId };
        var payment = await _mediator.Send(query);

        if (payment == null)
        {
            _logger.LogInformation("API: No payment found for order {OrderId}", orderId);
            return NotFound(new { error = $"Payment not found for order {orderId}" });
        }

        _logger.LogInformation(
            "API: Payment retrieved for order {OrderId}. Status: {Status}",
            orderId,
            payment.Status);

        return Ok(payment);
    }
}
