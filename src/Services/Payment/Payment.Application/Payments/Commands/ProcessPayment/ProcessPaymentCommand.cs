using MediatR;

namespace Payment.Application.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommand : IRequest<Guid>
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}
