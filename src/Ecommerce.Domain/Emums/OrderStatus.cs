namespace Ecommerce.Domain.Emums
{
    public enum OrderStatus
    {
        PendingPayment,
        PaymentFailed,
        Paid,
        ReadyToDispatch,
        Dispatched,
        Delivered,
        Cancelled
    }
}
