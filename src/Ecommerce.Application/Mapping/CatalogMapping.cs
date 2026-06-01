using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Catalog;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Mapping;

public static class CatalogMapping
{
    public static FamilyDto ToDto(this Family f) => new(
        f.Id, f.Name, f.Slug,
        f.Categories.Where(c => c.IsActive).Select(c => c.ToDto()).ToList());

    public static CategoryDto ToDto(this Category c) => new(
        c.Id, c.Name, c.Slug,
        c.Subcategories.Where(s => s.IsActive).Select(s => new SubcategoryDto(s.Id, s.Name, s.Slug)).ToList());

    public static ProductListItemDto ToListItem(this Product p) => new(
        p.Id, p.Name, p.Slug, p.BasePrice,
        p.Images.OrderBy(i => i.SortOrder).FirstOrDefault(i => i.IsPrimary)?.Url
            ?? p.Images.FirstOrDefault()?.Url);

    public static ProductDetailDto ToDetail(this Product p, IReadOnlyList<CatalogOptionDto> options) => new(
        p.Id, p.Name, p.Slug, p.Description, p.BasePrice,
        options,
        p.Variants.Where(v => v.IsActive).Select(v => new ProductVariantDto(
            v.Id, v.Sku, v.Price ?? p.BasePrice,
            (v.Inventory?.QuantityOnHand ?? 0) - (v.Inventory?.QuantityReserved ?? 0),
            v.OptionValues.Select(ov => ov.OptionValueId).ToList())).ToList(),
        p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList());

    public static CatalogOptionDto ToCatalogOption(this ProductOption o, IEnumerable<OptionValue>? values = null) => new(
        o.Id, o.Name, o.SortOrder,
        (values ?? o.Values).OrderBy(v => v.SortOrder)
            .Select(v => new CatalogOptionValueDto(v.Id, v.Value, v.Description, v.SortOrder)).ToList());

    public static PagedResult<ProductListItemDto> ToPaged(this (List<Product> Items, int Total) page, int pageNum, int pageSize) =>
        new(page.Items.Select(p => p.ToListItem()).ToList(), pageNum, pageSize, page.Total);
}
