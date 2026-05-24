using Ecommerce.Application.DTOs.Admin;

namespace Ecommerce.Application.Abstractions;

public interface IAdminDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default);
}
