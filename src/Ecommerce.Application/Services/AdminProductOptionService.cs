using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class AdminProductOptionService(IProductOptionRepository repo, IAdminCatalogRepository products) : IAdminProductOptionService
{
    public async Task<IReadOnlyList<ProductOptionDto>> ListByProductAsync(Guid productId, CancellationToken ct = default)
    {
        _ = await products.GetProductAsync(productId, ct) ?? throw new NotFoundException("Product", productId);
        var options = await repo.ListByProductAsync(productId, ct);
        return options.Select(Map).ToList();
    }

    public async Task<ProductOptionDto> SaveOptionAsync(Guid productId, Guid? optionId, SaveProductOptionRequest request, CancellationToken ct = default)
    {
        _ = await products.GetProductAsync(productId, ct) ?? throw new NotFoundException("Product", productId);
        var entity = new ProductOption
        {
            Id = optionId ?? Guid.Empty,
            ProductId = productId,
            Name = request.Name,
            SortOrder = request.SortOrder
        };
        var saved = await repo.SaveOptionAsync(entity, ct);
        return new ProductOptionDto(saved.Id, saved.ProductId, saved.Name, saved.SortOrder, []);
    }

    public Task DeleteOptionAsync(Guid productId, Guid optionId, CancellationToken ct = default) =>
        repo.DeleteOptionAsync(optionId, productId, ct);

    public async Task<OptionValueDto> SaveValueAsync(Guid productId, Guid optionId, Guid? valueId, SaveOptionValueRequest request, CancellationToken ct = default)
    {
        _ = await repo.GetAsync(optionId, productId, ct) ?? throw new NotFoundException("ProductOption", optionId);
        var value = new OptionValue
        {
            Id = valueId ?? Guid.Empty,
            ProductOptionId = optionId,
            Value = request.Value,
            SortOrder = request.SortOrder
        };
        var saved = await repo.SaveValueAsync(value, ct);
        return new OptionValueDto(saved.Id, saved.Value, saved.SortOrder);
    }

    public Task DeleteValueAsync(Guid productId, Guid optionId, Guid valueId, CancellationToken ct = default) =>
        repo.DeleteValueAsync(valueId, optionId, ct);

    private static ProductOptionDto Map(ProductOption o) => new(
        o.Id, o.ProductId, o.Name, o.SortOrder,
        o.Values.OrderBy(v => v.SortOrder).Select(v => new OptionValueDto(v.Id, v.Value, v.SortOrder)).ToList());
}
