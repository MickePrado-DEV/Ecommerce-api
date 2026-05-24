namespace Ecommerce.Application.DTOs.Checkout;

public record CheckoutRequest(
    string FullName, string Street, string City, string State,
    string PostalCode, string Country, string Phone, decimal ShippingCost);

public record CheckoutResultDto(Guid OrderId, string OrderNumber, decimal Total, string Status);
