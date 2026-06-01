using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<List<Order>> ListByUserAsync(Guid userId, CancellationToken ct = default);
    Task<(List<Order> Items, int Total)> ListAdminAsync(
        int page, int pageSize, OrderStatus? status, string? sortBy, string sortDirection, CancellationToken ct = default);
    Task<Order> AddAsync(Order order, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid orderId, OrderStatus status, CancellationToken ct = default);
    string GenerateOrderNumber();
}
