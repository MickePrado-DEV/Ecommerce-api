using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IProductOptionRepository
{
    Task<List<ProductOption>> ListByProductAsync(Guid productId, CancellationToken ct = default);
    Task<ProductOption?> GetAsync(Guid optionId, Guid productId, CancellationToken ct = default);
    Task<ProductOption> SaveOptionAsync(ProductOption option, CancellationToken ct = default);
    Task DeleteOptionAsync(Guid optionId, Guid productId, CancellationToken ct = default);
    Task<OptionValue> SaveValueAsync(OptionValue value, CancellationToken ct = default);
    Task DeleteValueAsync(Guid valueId, Guid optionId, CancellationToken ct = default);
}
