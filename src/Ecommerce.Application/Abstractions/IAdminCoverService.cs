using Ecommerce.Application.DTOs.Admin;

namespace Ecommerce.Application.Abstractions;

public interface IAdminCoverService
{
    Task<IReadOnlyList<CoverAdminDto>> ListAsync(CancellationToken ct = default);
    Task<CoverAdminDto?> GetAsync(Guid id, CancellationToken ct = default);
    Task<CoverAdminDto> SaveAsync(SaveCoverRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ReorderAsync(ReorderCoversRequest request, CancellationToken ct = default);
}
