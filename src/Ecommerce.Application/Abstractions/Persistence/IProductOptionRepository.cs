using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IProductOptionRepository
{
    Task<List<ProductOption>> ListAllAsync(CancellationToken ct = default);
    Task<ProductOption?> GetByIdAsync(Guid optionId, CancellationToken ct = default);
    Task<ProductOption> SaveOptionAsync(ProductOption option, CancellationToken ct = default);
    Task DeleteOptionAsync(Guid optionId, CancellationToken ct = default);
    Task<OptionValue> SaveValueAsync(OptionValue value, CancellationToken ct = default);
    Task DeleteValueAsync(Guid valueId, Guid optionId, CancellationToken ct = default);
    Task<bool> HasAssignmentsAsync(Guid optionId, CancellationToken ct = default);

    Task<List<ProductOptionAssignment>> ListAssignmentsAsync(Guid productId, CancellationToken ct = default);
    Task AttachOptionAsync(Guid productId, Guid optionId, IReadOnlyList<Guid> valueIds, CancellationToken ct = default);
    Task DetachOptionAsync(Guid productId, Guid optionId, CancellationToken ct = default);

    Task<GenerateVariantsResultDto> GenerateVariantsAsync(Guid productId, CancellationToken ct = default);
    Task<List<Variant>> ListVariantsAsync(Guid productId, CancellationToken ct = default);
}
