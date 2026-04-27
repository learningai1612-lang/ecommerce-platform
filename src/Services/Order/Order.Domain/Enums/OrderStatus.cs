namespace Order.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    PaymentPending = 1,
    Paid = 2,
    Failed = 3,
    Cancelled = 4,
    Shipped = 5,
    Completed = 6
}
