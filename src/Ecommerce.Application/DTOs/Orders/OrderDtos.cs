using Ecommerce.Application.DTOs.Dispatch;

namespace Ecommerce.Application.DTOs.Orders;

public record OrderSummaryDto(Guid Id, string OrderNumber, string Status, decimal Total, DateTime CreatedAt);
public record OrderShipmentInfoDto(
    Guid ShipmentId,
    string Status,
    string? TrackingNumber,
    string? DriverName,
    DateTime? ShippedAt);

public record OrderDetailDto(
    Guid Id,
    string OrderNumber,
    string Status,
    decimal Subtotal,
    decimal DiscountAmount,
    string? CouponCode,
    decimal ShippingCost,
    decimal Total,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDto> Items,
    OrderAddressDto? Address,
    PaymentInfoDto? Payment,
    OrderShipmentInfoDto? Shipment = null,
    OrderDispatchInfoDto? Dispatch = null);
public record OrderItemDto(
    string ProductName,
    string Sku,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    Guid? ProductId = null,
    string? ProductSlug = null);
public record OrderAddressDto(
    string FullName,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    string Phone,
    decimal? Latitude = null,
    decimal? Longitude = null,
    string? AddressText = null);
public record PaymentInfoDto(string Status, decimal Amount, DateTime? PaidAt);
public record PaymentResultDto(Guid OrderId, string Status, string? Reference);
public record PagedOrdersDto(IReadOnlyList<OrderSummaryDto> Items, int Total, int Page, int PageSize);

public record OrderTrackingDto(
    Guid OrderId,
    string OrderNumber,
    string OrderStatus,
    OrderShipmentInfoDto? Shipment,
    string? DispatchStatus = null,
    string? DispatchDriverName = null);

public record PagedOrdersAdminDto(IReadOnlyList<OrderSummaryDto> Items, int Total, int Page, int PageSize);
