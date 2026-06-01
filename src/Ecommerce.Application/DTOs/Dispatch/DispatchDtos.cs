namespace Ecommerce.Application.DTOs.Dispatch;

public record DispatchSettingsDto(
    decimal DefaultClusterRadiusKm,
    int DefaultMaxStopsPerRoute,
    int DefaultMaxStopsPerBatch,
    string DefaultRouteOriginType,
    bool AllowOriginSelection);

public record UpdateDispatchSettingsRequest(
    decimal? DefaultClusterRadiusKm,
    int? DefaultMaxStopsPerRoute,
    int? DefaultMaxStopsPerBatch,
    string? DefaultRouteOriginType,
    bool? AllowOriginSelection);

public record DispatchQueueOrderDto(
    Guid Id,
    string OrderNumber,
    string Status,
    string DispatchStatus,
    decimal Total,
    DateTime CreatedAt,
    string? AddressText,
    decimal? Latitude,
    decimal? Longitude);

public record DispatchQueueFilter(
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 50);

public record PagedDispatchQueueDto(
    IReadOnlyList<DispatchQueueOrderDto> Items,
    int Total,
    int Page,
    int PageSize);

public record AutoCreateBatchesRequest(
    double? RadiusKm,
    int? MaxStops,
    DateTime? From,
    DateTime? To);

public record ManualCreateBatchRequest(
    IReadOnlyList<Guid> OrderIds,
    double? RadiusKm,
    int? MaxStops);

public record DispatchBatchSummaryDto(
    Guid Id,
    string Code,
    string Status,
    int OrderCount,
    double CenterLat,
    double CenterLng,
    double RadiusKm,
    int MaxStops,
    DateTime CreatedAt);

public record DispatchBatchOrderItemDto(
    Guid OrderId,
    string OrderNumber,
    double? DistanceKm,
    string? AddressText);

public record DispatchBatchDetailDto(
    Guid Id,
    string Code,
    string Status,
    double CenterLat,
    double CenterLng,
    double RadiusKm,
    int MaxStops,
    DateTime CreatedAt,
    IReadOnlyList<DispatchBatchOrderItemDto> Orders);

public record CreateRouteFromBatchRequest(
    string OriginType,
    double? OriginLat,
    double? OriginLng,
    Guid? DriverId,
    int? MaxStops);

public record DeliveryRouteSummaryDto(
    Guid Id,
    string Code,
    string Status,
    int TotalStops,
    double? TotalDistanceKm,
    string? DriverName,
    DateTime CreatedAt);

public record DeliveryRouteStopDto(
    Guid Id,
    int StopIndex,
    Guid OrderId,
    string OrderNumber,
    string AddressText,
    double Lat,
    double Lng,
    string Status,
    DateTime? DeliveredAt,
    DateTime? FailedAt);

public record DeliveryRouteDetailDto(
    Guid Id,
    string Code,
    string Status,
    string OriginType,
    double? OriginLat,
    double? OriginLng,
    int TotalStops,
    double? TotalDistanceKm,
    Guid? DriverId,
    string? DriverName,
    Guid? BatchId,
    string? BatchCode,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    IReadOnlyList<DeliveryRouteStopDto> Stops,
    string? GoogleMapsUrl);

public record AssignRouteRequest(Guid DriverId);

public record FailStopRequest(string? Reason);

public record OrderDispatchInfoDto(
    string DispatchStatus,
    string? BatchCode,
    string? RouteCode,
    string? DriverName,
    bool CanMarkReady);

public record CreateBatchesResultDto(int BatchesCreated, IReadOnlyList<string> BatchCodes);
