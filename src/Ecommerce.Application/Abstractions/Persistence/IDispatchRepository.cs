using Ecommerce.Application.DTOs.Dispatch;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IDispatchRepository
{
    Task<DispatchSettings> GetOrCreateSettingsAsync(CancellationToken ct = default);
    Task UpdateSettingsAsync(DispatchSettings settings, CancellationToken ct = default);

    Task<(List<Order> Items, int Total)> ListReadyQueueAsync(DispatchQueueFilter filter, CancellationToken ct = default);
    Task<List<Order>> GetReadyOrdersForBatchingAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<bool> IsOrderInActiveBatchAsync(Guid orderId, CancellationToken ct = default);
    Task<HashSet<Guid>> GetOrderIdsInBatchesAsync(CancellationToken ct = default);

    Task<string> NextBatchCodeAsync(CancellationToken ct = default);
    Task<string> NextRouteCodeAsync(CancellationToken ct = default);
    Task AddBatchAsync(DispatchBatch batch, IEnumerable<DispatchBatchOrder> pivots, IEnumerable<Order> ordersToUpdate, CancellationToken ct = default);

    Task<List<DispatchBatch>> ListBatchesAsync(CancellationToken ct = default);
    Task<DispatchBatch?> GetBatchDetailAsync(Guid batchId, CancellationToken ct = default);

    Task AddRouteAsync(DeliveryRoute route, IEnumerable<DeliveryRouteStop> stops, IEnumerable<Order> ordersToUpdate, DispatchBatch? batchToUpdate, CancellationToken ct = default);

    Task<List<DeliveryRoute>> ListRoutesAsync(CancellationToken ct = default);
    Task<DeliveryRoute?> GetRouteDetailAsync(Guid routeId, CancellationToken ct = default);
    Task<DeliveryRoute?> GetRouteByStopIdAsync(Guid stopId, CancellationToken ct = default);

    Task AssignRouteAsync(DeliveryRoute route, IEnumerable<Order> orders, CancellationToken ct = default);
    Task StartRouteAsync(DeliveryRoute route, IEnumerable<Order> orders, CancellationToken ct = default);
    Task FinishRouteAsync(DeliveryRoute route, IEnumerable<Order> orders, CancellationToken ct = default);
    Task UpdateStopDeliveredAsync(DeliveryRouteStop stop, Order order, CancellationToken ct = default);
    Task UpdateStopFailedAsync(DeliveryRouteStop stop, Order order, string? reason, CancellationToken ct = default);

    Task<OrderDispatchInfoDto?> GetOrderDispatchInfoAsync(Guid orderId, CancellationToken ct = default);
    Task MarkOrderDispatchReadyAsync(Order order, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken ct = default);
}
