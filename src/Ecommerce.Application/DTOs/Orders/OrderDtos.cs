namespace Ecommerce.Application.DTOs.Orders;

public record OrderSummaryDto(Guid Id, string OrderNumber, string Status, decimal Total, DateTime CreatedAt);
public record OrderDetailDto(Guid Id, string OrderNumber, string Status, decimal Subtotal, decimal ShippingCost, decimal Total, DateTime CreatedAt, IReadOnlyList<OrderItemDto> Items, OrderAddressDto? Address, PaymentInfoDto? Payment);
public record OrderItemDto(string ProductName, string Sku, int Quantity, decimal UnitPrice, decimal LineTotal);
public record OrderAddressDto(string FullName, string Street, string City, string State, string PostalCode, string Country, string Phone);
public record PaymentInfoDto(string Status, decimal Amount, DateTime? PaidAt);
public record PaymentResultDto(Guid OrderId, string Status, string? Reference);
public record PagedOrdersAdminDto(IReadOnlyList<OrderSummaryDto> Items, int Total, int Page, int PageSize);
