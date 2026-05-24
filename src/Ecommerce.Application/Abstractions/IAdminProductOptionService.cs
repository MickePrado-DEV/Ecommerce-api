using Ecommerce.Application.DTOs.Admin;

namespace Ecommerce.Application.Abstractions;

public interface IAdminProductOptionService
{
    Task<IReadOnlyList<ProductOptionDto>> ListByProductAsync(Guid productId, CancellationToken ct = default);
    Task<ProductOptionDto> SaveOptionAsync(Guid productId, Guid? optionId, SaveProductOptionRequest request, CancellationToken ct = default);
    Task DeleteOptionAsync(Guid productId, Guid optionId, CancellationToken ct = default);
    Task<OptionValueDto> SaveValueAsync(Guid productId, Guid optionId, Guid? valueId, SaveOptionValueRequest request, CancellationToken ct = default);
    Task DeleteValueAsync(Guid productId, Guid optionId, Guid valueId, CancellationToken ct = default);
}
