using Ecommerce.Application.DTOs.Orders;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IOrderReadRepository
{
    Task<(IReadOnlyList<OrderSummaryDto> Items, int Total)> ListSummariesByUserAsync(
        Guid userId, int page, int pageSize, string? status, CancellationToken ct = default);
    Task<OrderDetailDto?> GetDetailForUserAsync(Guid orderId, Guid userId, CancellationToken ct = default);
    Task<OrderTrackingDto?> GetTrackingForUserAsync(Guid orderId, Guid userId, CancellationToken ct = default);
}
