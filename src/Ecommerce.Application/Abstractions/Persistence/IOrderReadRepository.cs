using Ecommerce.Application.DTOs.Orders;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IOrderReadRepository
{
    Task<IReadOnlyList<OrderSummaryDto>> ListSummariesByUserAsync(Guid userId, CancellationToken ct = default);
    Task<OrderDetailDto?> GetDetailForUserAsync(Guid orderId, Guid userId, CancellationToken ct = default);
}
