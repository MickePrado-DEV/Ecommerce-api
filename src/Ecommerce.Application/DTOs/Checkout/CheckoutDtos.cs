namespace Ecommerce.Application.DTOs.Checkout;

public record CheckoutRequest(
    Guid? AddressId,
    string? FullName,
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string? Phone,
    decimal ShippingCost,
    string? CouponCode = null);

public record CheckoutResultDto(
    Guid OrderId,
    string OrderNumber,
    decimal Subtotal,
    decimal DiscountAmount,
    string? CouponCode,
    decimal Total,
    string Status);
