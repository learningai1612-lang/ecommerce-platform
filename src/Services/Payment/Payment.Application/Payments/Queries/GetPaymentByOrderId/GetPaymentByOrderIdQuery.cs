using MediatR;
using Payment.Application.Common.DTOs;

namespace Payment.Application.Payments.Queries.GetPaymentByOrderId;

public class GetPaymentByOrderIdQuery : IRequest<PaymentResponseDto?>
{
    public Guid OrderId { get; set; }
}
