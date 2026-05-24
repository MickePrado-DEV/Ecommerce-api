using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Admin;

namespace Ecommerce.Application.Services;

public class AdminDashboardService(IDashboardRepository repo) : IAdminDashboardService
{
    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var s = await repo.GetStatsAsync(ct);
        return new DashboardStatsDto(s.Orders, s.PendingPayment, s.Paid, s.ReadyToDispatch, s.Products, s.Users);
    }
}
