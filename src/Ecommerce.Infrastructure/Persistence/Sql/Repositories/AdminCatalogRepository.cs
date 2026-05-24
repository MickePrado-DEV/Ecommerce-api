using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class AdminCatalogRepository(EcommerceDbContext db) : IAdminCatalogRepository
{
    public Task<Family?> GetFamilyAsync(Guid id, CancellationToken ct = default) =>
        db.Families.FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<List<Family>> ListFamiliesAsync(CancellationToken ct = default) =>
        db.Families.OrderBy(f => f.SortOrder).ToListAsync(ct);

    public async Task<Family> SaveFamilyAsync(Family entity, CancellationToken ct = default)
    {
        if (entity.Id == Guid.Empty) db.Families.Add(entity);
        else db.Families.Update(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteFamilyAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Families.FindAsync([id], ct) ?? throw new NotFoundException("Family", id);
        db.Families.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public Task<Category?> GetCategoryAsync(Guid id, CancellationToken ct = default) =>
        db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Category> SaveCategoryAsync(Category entity, CancellationToken ct = default)
    {
        if (entity.Id == Guid.Empty) db.Categories.Add(entity);
        else db.Categories.Update(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Categories.FindAsync([id], ct) ?? throw new NotFoundException("Category", id);
        db.Categories.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public Task<Subcategory?> GetSubcategoryAsync(Guid id, CancellationToken ct = default) =>
        db.Subcategories.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Subcategory> SaveSubcategoryAsync(Subcategory entity, CancellationToken ct = default)
    {
        if (entity.Id == Guid.Empty) db.Subcategories.Add(entity);
        else db.Subcategories.Update(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteSubcategoryAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Subcategories.FindAsync([id], ct) ?? throw new NotFoundException("Subcategory", id);
        db.Subcategories.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public Task<Product?> GetProductAsync(Guid id, CancellationToken ct = default) =>
        db.Products.Include(p => p.Variants).Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(List<Product> Items, int Total)> ListProductsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var total = await db.Products.CountAsync(ct);
        var items = await db.Products.OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Product> SaveProductAsync(Product entity, CancellationToken ct = default)
    {
        if (entity.Id == Guid.Empty) db.Products.Add(entity);
        else db.Products.Update(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteProductAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Products.FindAsync([id], ct) ?? throw new NotFoundException("Product", id);
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    public Task<Variant?> GetVariantAsync(Guid id, CancellationToken ct = default) =>
        db.Variants.Include(v => v.Inventory).FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<Variant> SaveVariantAsync(Variant entity, CancellationToken ct = default)
    {
        if (entity.Id == Guid.Empty)
        {
            db.Variants.Add(entity);
            await db.SaveChangesAsync(ct);
            db.Inventories.Add(new Inventory { VariantId = entity.Id, QuantityOnHand = 0 });
        }
        else db.Variants.Update(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteVariantAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Variants.FindAsync([id], ct) ?? throw new NotFoundException("Variant", id);
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }
}
