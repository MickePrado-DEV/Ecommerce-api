using Ecommerce.Application.DTOs.Orders;

namespace Ecommerce.Application.Abstractions;

public interface IOrderService
{
    Task<OrderDetailDto?> GetAsync(Guid userId, Guid orderId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderSummaryDto>> ListMineAsync(Guid userId, CancellationToken ct = default);
    Task<PaymentResultDto> PayMockAsync(Guid userId, Guid orderId, CancellationToken ct = default);
}
