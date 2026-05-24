using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Application.Abstractions;

public interface IAdminOrderService
{
    Task<PagedOrdersAdminDto> ListAsync(int page, int pageSize, OrderStatus? status, CancellationToken ct = default);
    Task<OrderDetailDto?> GetAsync(Guid orderId, CancellationToken ct = default);
    Task MarkReadyToDispatchAsync(Guid orderId, CancellationToken ct = default);
    Task<byte[]> GenerateTicketPdfByOrderAsync(Guid orderId, CancellationToken ct = default);
}
