// Handlers admin: dashboard, covers, catálogo CRUD, inventario, pedidos, envíos, opciones de producto.
using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Application.DTOs.Shipments;
using Ecommerce.Application.Features.Orders;
using Ecommerce.Domain.Admin;
using Ecommerce.Domain.Covers;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Admin;

// --- Dashboard ---

public record GetDashboardStatsQuery : IRequest<Result<DashboardStatsDto>>;

public class GetDashboardStatsQueryHandler(IDashboardRepository repo)
    : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        var s = await repo.GetStatsAsync(ct);
        return Result.Ok(new DashboardStatsDto(s.Orders, s.PendingPayment, s.Paid, s.ReadyToDispatch, s.Products, s.Users));
    }
}

// --- Covers ---

public record ListCoversAdminQuery : IRequest<Result<IReadOnlyList<CoverAdminDto>>>;

public class ListCoversAdminQueryHandler(ICoverRepository repo)
    : IRequestHandler<ListCoversAdminQuery, Result<IReadOnlyList<CoverAdminDto>>>
{
    public async Task<Result<IReadOnlyList<CoverAdminDto>>> Handle(ListCoversAdminQuery request, CancellationToken ct)
    {
        await repo.DeactivateExpiredAsync(ct);
        var now = DateTime.UtcNow;
        var list = await repo.ListAllAsync(ct);
        return Result.Ok((IReadOnlyList<CoverAdminDto>)list.Select(c => AdminCoverMapping.Map(c, now)).ToList());
    }
}

public record ListCoversPagedAdminQuery(int Page, int PageSize) : IRequest<Result<PagedCoversAdminDto>>;

public class ListCoversPagedAdminQueryHandler(ICoverRepository repo)
    : IRequestHandler<ListCoversPagedAdminQuery, Result<PagedCoversAdminDto>>
{
    public async Task<Result<PagedCoversAdminDto>> Handle(ListCoversPagedAdminQuery request, CancellationToken ct)
    {
        await repo.DeactivateExpiredAsync(ct);
        var now = DateTime.UtcNow;
        var (items, total) = await repo.ListPagedAsync(request.Page, request.PageSize, ct);
        return Result.Ok(new PagedCoversAdminDto(
            items.Select(c => AdminCoverMapping.Map(c, now)).ToList(),
            total,
            request.Page,
            request.PageSize));
    }
}

public record GetCoverAdminQuery(Guid Id) : IRequest<Result<CoverAdminDto>>;

public class GetCoverAdminQueryHandler(ICoverRepository repo)
    : IRequestHandler<GetCoverAdminQuery, Result<CoverAdminDto>>
{
    public async Task<Result<CoverAdminDto>> Handle(GetCoverAdminQuery request, CancellationToken ct)
    {
        var cover = await repo.GetAsync(request.Id, ct);
        if (cover is null)
            return Result.Fail<CoverAdminDto>(AdminErrors.NotFound("Cover", request.Id));

        var now = DateTime.UtcNow;
        return Result.Ok(AdminCoverMapping.Map(cover, now));
    }
}

public record SaveCoverCommand(
    Guid? Id,
    string Title,
    string ImageUrl,
    string? LinkUrl,
    bool IsActive,
    DateTime? StartsAt,
    DateTime? EndsAt)
    : IRequest<Result<CoverAdminDto>>;

public class SaveCoverCommandHandler(ICoverRepository repo, ICoverImageStorage imageStorage)
    : IRequestHandler<SaveCoverCommand, Result<CoverAdminDto>>
{
    public async Task<Result<CoverAdminDto>> Handle(SaveCoverCommand request, CancellationToken ct)
    {
        await repo.DeactivateExpiredAsync(ct);
        var now = DateTime.UtcNow;

        var wantActive = request.IsActive;
        if (request.EndsAt.HasValue && request.EndsAt.Value < now)
            wantActive = false;

        Cover? existing = null;
        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            existing = await repo.GetAsync(request.Id.Value, ct);
            if (existing is null)
                return Result.Fail<CoverAdminDto>(AdminErrors.NotFound("Cover", request.Id.Value));

            if (!string.Equals(existing.ImageUrl, request.ImageUrl, StringComparison.OrdinalIgnoreCase))
                imageStorage.TryDeleteByUrl(existing.ImageUrl);
        }

        var sortOrder = 0;
        if (wantActive)
        {
            var activeCount = await repo.CountEffectiveActiveAsync(existing?.Id, ct);
            var alreadyEffective = existing is not null && CoverRules.IsEffectivelyActive(existing, now);

            if (!alreadyEffective && activeCount >= CoverRules.MaxPrincipalActive)
            {
                return Result.Fail<CoverAdminDto>(AdminErrors.InvalidState(
                    $"Solo puede haber {CoverRules.MaxPrincipalActive} portadas activas y vigentes."));
            }

            if (existing is not null && existing.SortOrder is >= 1 and <= CoverRules.MaxPrincipalActive)
                sortOrder = existing.SortOrder;
            else
            {
                var next = await repo.GetNextPrincipalOrderAsync(ct);
                if (next is null)
                {
                    return Result.Fail<CoverAdminDto>(AdminErrors.InvalidState(
                        $"Solo puede haber {CoverRules.MaxPrincipalActive} portadas activas y vigentes."));
                }

                sortOrder = next.Value;
            }
        }

        var entity = new Cover
        {
            Id = request.Id ?? Guid.Empty,
            Title = request.Title,
            ImageUrl = request.ImageUrl,
            LinkUrl = request.LinkUrl,
            SortOrder = sortOrder,
            IsActive = wantActive,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt
        };

        var saved = await repo.SaveAsync(entity, ct);
        return Result.Ok(AdminCoverMapping.Map(saved, now));
    }
}

public record DeleteCoverCommand(Guid Id) : IRequest<Result>;

public class DeleteCoverCommandHandler(ICoverRepository repo, ICoverImageStorage imageStorage)
    : IRequestHandler<DeleteCoverCommand, Result>
{
    public async Task<Result> Handle(DeleteCoverCommand request, CancellationToken ct)
    {
        var cover = await repo.GetAsync(request.Id, ct);
        if (cover is null)
            return Result.Fail(AdminErrors.NotFound("Cover", request.Id));

        imageStorage.TryDeleteByUrl(cover.ImageUrl);
        await repo.DeleteAsync(request.Id, ct);
        return Result.Ok();
    }
}

public record ReorderCoversCommand(IReadOnlyList<Guid> Ids) : IRequest<Result>;

public class ReorderCoversCommandHandler(ICoverRepository repo) : IRequestHandler<ReorderCoversCommand, Result>
{
    public async Task<Result> Handle(ReorderCoversCommand request, CancellationToken ct)
    {
        if (request.Ids.Count > CoverRules.MaxPrincipalActive)
        {
            return Result.Fail(AdminErrors.InvalidState(
                $"Máximo {CoverRules.MaxPrincipalActive} portadas en el orden principal."));
        }

        try
        {
            await repo.DeactivateExpiredAsync(ct);
            await repo.ReorderPrincipalAsync(request.Ids, ct);
            return Result.Ok();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(AdminErrors.InvalidState(ex.Message));
        }
    }
}

internal static class AdminCoverMapping
{
    public static CoverAdminDto Map(Cover c, DateTime utcNow) => new(
        c.Id,
        c.Title,
        c.ImageUrl,
        c.LinkUrl,
        c.SortOrder,
        c.IsActive,
        c.StartsAt,
        c.EndsAt,
        CoverRules.IsEffectivelyActive(c, utcNow));
}

// --- Catalog ---

public record ListFamiliesAdminQuery : IRequest<Result<IReadOnlyList<FamilyAdminDto>>>;

public class ListFamiliesAdminQueryHandler(IAdminCatalogRepository repo)
    : IRequestHandler<ListFamiliesAdminQuery, Result<IReadOnlyList<FamilyAdminDto>>>
{
    public async Task<Result<IReadOnlyList<FamilyAdminDto>>> Handle(ListFamiliesAdminQuery request, CancellationToken ct)
    {
        var items = await repo.ListFamiliesAsync(ct);
        return Result.Ok((IReadOnlyList<FamilyAdminDto>)items
            .Select(f => new FamilyAdminDto(f.Id, f.Name, f.Slug, f.SortOrder, f.IsActive)).ToList());
    }
}

public record SaveFamilyCommand(Guid? Id, string Name, string Slug, int SortOrder, bool IsActive)
    : IRequest<Result<FamilyAdminDto>>;

public class SaveFamilyCommandHandler(IAdminCatalogRepository repo)
    : IRequestHandler<SaveFamilyCommand, Result<FamilyAdminDto>>
{
    public async Task<Result<FamilyAdminDto>> Handle(SaveFamilyCommand request, CancellationToken ct)
    {
        var saved = await repo.SaveFamilyAsync(new Family
        {
            Id = request.Id ?? Guid.Empty,
            Name = request.Name,
            Slug = request.Slug,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        }, ct);
        return Result.Ok(new FamilyAdminDto(saved.Id, saved.Name, saved.Slug, saved.SortOrder, saved.IsActive));
    }
}

public record DeleteFamilyCommand(Guid Id) : IRequest<Result>;

public class DeleteFamilyCommandHandler(IAdminCatalogRepository repo) : IRequestHandler<DeleteFamilyCommand, Result>
{
    public async Task<Result> Handle(DeleteFamilyCommand request, CancellationToken ct)
    {
        await repo.DeleteFamilyAsync(request.Id, ct);
        return Result.Ok();
    }
}

public record SaveCategoryCommand(Guid? Id, Guid FamilyId, string Name, string Slug, int SortOrder, bool IsActive)
    : IRequest<Result<CategoryAdminDto>>;

public class SaveCategoryCommandHandler(IAdminCatalogRepository repo)
    : IRequestHandler<SaveCategoryCommand, Result<CategoryAdminDto>>
{
    public async Task<Result<CategoryAdminDto>> Handle(SaveCategoryCommand request, CancellationToken ct)
    {
        var saved = await repo.SaveCategoryAsync(new Category
        {
            Id = request.Id ?? Guid.Empty,
            FamilyId = request.FamilyId,
            Name = request.Name,
            Slug = request.Slug,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        }, ct);
        return Result.Ok(new CategoryAdminDto(saved.Id, saved.FamilyId, saved.Name, saved.Slug, saved.SortOrder, saved.IsActive));
    }
}

public record DeleteCategoryCommand(Guid Id) : IRequest<Result>;

public class DeleteCategoryCommandHandler(IAdminCatalogRepository repo) : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        await repo.DeleteCategoryAsync(request.Id, ct);
        return Result.Ok();
    }
}

public record SaveSubcategoryCommand(Guid? Id, Guid CategoryId, string Name, string Slug, int SortOrder, bool IsActive)
    : IRequest<Result<SubcategoryAdminDto>>;

public class SaveSubcategoryCommandHandler(IAdminCatalogRepository repo)
    : IRequestHandler<SaveSubcategoryCommand, Result<SubcategoryAdminDto>>
{
    public async Task<Result<SubcategoryAdminDto>> Handle(SaveSubcategoryCommand request, CancellationToken ct)
    {
        var saved = await repo.SaveSubcategoryAsync(new Subcategory
        {
            Id = request.Id ?? Guid.Empty,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Slug = request.Slug,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        }, ct);
        return Result.Ok(new SubcategoryAdminDto(saved.Id, saved.CategoryId, saved.Name, saved.Slug, saved.SortOrder, saved.IsActive));
    }
}

public record DeleteSubcategoryCommand(Guid Id) : IRequest<Result>;

public class DeleteSubcategoryCommandHandler(IAdminCatalogRepository repo) : IRequestHandler<DeleteSubcategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteSubcategoryCommand request, CancellationToken ct)
    {
        await repo.DeleteSubcategoryAsync(request.Id, ct);
        return Result.Ok();
    }
}

public record ListProductsAdminQuery(int Page, int PageSize) : IRequest<Result<PagedProductsAdminDto>>;

public class ListProductsAdminQueryHandler(IAdminCatalogRepository repo)
    : IRequestHandler<ListProductsAdminQuery, Result<PagedProductsAdminDto>>
{
    public async Task<Result<PagedProductsAdminDto>> Handle(ListProductsAdminQuery request, CancellationToken ct)
    {
        var result = await repo.ListProductsAsync(request.Page, request.PageSize, ct);
        return Result.Ok(new PagedProductsAdminDto(
            result.Items.Select(p => new ProductAdminDto(p.Id, p.SubcategoryId, p.Name, p.Slug, p.Description, p.BasePrice, p.IsActive)).ToList(),
            result.Total, request.Page, request.PageSize));
    }
}

public record SaveProductCommand(Guid? Id, Guid SubcategoryId, string Name, string Slug, string? Description, decimal BasePrice, bool IsActive)
    : IRequest<Result<ProductAdminDto>>;

public class SaveProductCommandHandler(IAdminCatalogRepository repo)
    : IRequestHandler<SaveProductCommand, Result<ProductAdminDto>>
{
    public async Task<Result<ProductAdminDto>> Handle(SaveProductCommand request, CancellationToken ct)
    {
        var saved = await repo.SaveProductAsync(new Product
        {
            Id = request.Id ?? Guid.Empty,
            SubcategoryId = request.SubcategoryId,
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            BasePrice = request.BasePrice,
            IsActive = request.IsActive
        }, ct);
        return Result.Ok(new ProductAdminDto(saved.Id, saved.SubcategoryId, saved.Name, saved.Slug, saved.Description, saved.BasePrice, saved.IsActive));
    }
}

public record DeleteProductCommand(Guid Id) : IRequest<Result>;

public class DeleteProductCommandHandler(IAdminCatalogRepository repo) : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        await repo.DeleteProductAsync(request.Id, ct);
        return Result.Ok();
    }
}

public record SaveVariantCommand(Guid? Id, Guid ProductId, string Sku, decimal? Price, bool IsActive, int? InitialStock)
    : IRequest<Result<VariantAdminDto>>;

public class SaveVariantCommandHandler(IAdminCatalogRepository repo, IInventoryRepository inventory)
    : IRequestHandler<SaveVariantCommand, Result<VariantAdminDto>>
{
    public async Task<Result<VariantAdminDto>> Handle(SaveVariantCommand request, CancellationToken ct)
    {
        var saved = await repo.SaveVariantAsync(new Variant
        {
            Id = request.Id ?? Guid.Empty,
            ProductId = request.ProductId,
            Sku = request.Sku,
            Price = request.Price,
            IsActive = request.IsActive
        }, ct);
        if (request.InitialStock.HasValue)
            await inventory.UpsertAsync(saved.Id, request.InitialStock.Value, ct);
        var inv = await inventory.GetByVariantIdAsync(saved.Id, ct);
        return Result.Ok(new VariantAdminDto(saved.Id, saved.ProductId, saved.Sku, saved.Price, saved.IsActive, inv?.QuantityOnHand ?? 0));
    }
}

public record DeleteVariantCommand(Guid Id) : IRequest<Result>;

public class DeleteVariantCommandHandler(IAdminCatalogRepository repo) : IRequestHandler<DeleteVariantCommand, Result>
{
    public async Task<Result> Handle(DeleteVariantCommand request, CancellationToken ct)
    {
        await repo.DeleteVariantAsync(request.Id, ct);
        return Result.Ok();
    }
}

// --- Inventory ---

public record ListInventoryQuery : IRequest<Result<IReadOnlyList<InventoryDto>>>;

public class ListInventoryQueryHandler(IInventoryRepository inventory)
    : IRequestHandler<ListInventoryQuery, Result<IReadOnlyList<InventoryDto>>>
{
    public async Task<Result<IReadOnlyList<InventoryDto>>> Handle(ListInventoryQuery request, CancellationToken ct)
    {
        var items = await inventory.ListAsync(ct);
        return Result.Ok((IReadOnlyList<InventoryDto>)items.Select(i => new InventoryDto(
            i.VariantId, i.Variant.Sku, i.Variant.Product.Name,
            i.QuantityOnHand, i.QuantityReserved,
            i.QuantityOnHand - i.QuantityReserved)).ToList());
    }
}

public record SetInventoryCommand(Guid VariantId, int QuantityOnHand) : IRequest<Result<InventoryDto>>;

public class SetInventoryCommandHandler(IInventoryRepository inventory)
    : IRequestHandler<SetInventoryCommand, Result<InventoryDto>>
{
    public async Task<Result<InventoryDto>> Handle(SetInventoryCommand request, CancellationToken ct)
    {
        await inventory.UpsertAsync(request.VariantId, request.QuantityOnHand, ct);
        var inv = await inventory.GetByVariantIdAsync(request.VariantId, ct);
        if (inv is null)
            return Result.Fail<InventoryDto>(AdminErrors.NotFound("Inventory", request.VariantId));
        return Result.Ok(new InventoryDto(request.VariantId, inv.Variant.Sku, inv.Variant.Product.Name,
            inv.QuantityOnHand, inv.QuantityReserved, inv.QuantityOnHand - inv.QuantityReserved));
    }
}

// --- Orders (admin) ---

public record ListAdminOrdersQuery(int Page, int PageSize, OrderStatus? Status)
    : IRequest<Result<PagedOrdersAdminDto>>;

public class ListAdminOrdersQueryHandler(IOrderRepository orders)
    : IRequestHandler<ListAdminOrdersQuery, Result<PagedOrdersAdminDto>>
{
    public async Task<Result<PagedOrdersAdminDto>> Handle(ListAdminOrdersQuery request, CancellationToken ct)
    {
        var result = await orders.ListAdminAsync(request.Page, request.PageSize, request.Status, ct);
        return Result.Ok(new PagedOrdersAdminDto(
            result.Items.Select(o => new OrderSummaryDto(o.Id, o.OrderNumber, o.Status.ToString(), o.Total, o.CreatedAt)).ToList(),
            result.Total, request.Page, request.PageSize));
    }
}

public record GetAdminOrderQuery(Guid OrderId) : IRequest<Result<OrderDetailDto>>;

public class GetAdminOrderQueryHandler(IOrderRepository orders)
    : IRequestHandler<GetAdminOrderQuery, Result<OrderDetailDto>>
{
    public async Task<Result<OrderDetailDto>> Handle(GetAdminOrderQuery request, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(request.OrderId, ct);
        return order is null
            ? Result.Fail<OrderDetailDto>(AdminErrors.NotFound("Order", request.OrderId))
            : Result.Ok(OrderMapping.ToDetail(order));
    }
}

public record MarkOrderReadyToDispatchCommand(Guid OrderId) : IRequest<Result>;

public class MarkOrderReadyToDispatchCommandHandler(IOrderRepository orders)
    : IRequestHandler<MarkOrderReadyToDispatchCommand, Result>
{
    public async Task<Result> Handle(MarkOrderReadyToDispatchCommand request, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(request.OrderId, ct);
        if (order is null)
            return Result.Fail(AdminErrors.NotFound("Order", request.OrderId));
        if (order.Status != OrderStatus.Paid)
            return Result.Fail(AdminErrors.InvalidState("La orden debe estar pagada"));
        await orders.UpdateStatusAsync(request.OrderId, OrderStatus.ReadyToDispatch, ct);
        return Result.Ok();
    }
}

public record GenerateOrderTicketPdfQuery(Guid OrderId) : IRequest<Result<byte[]>>;

public class GenerateOrderTicketPdfQueryHandler(IShipmentRepository shipments, IPdfTicketGenerator pdf)
    : IRequestHandler<GenerateOrderTicketPdfQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(GenerateOrderTicketPdfQuery request, CancellationToken ct)
    {
        var shipment = await shipments.GetByOrderIdAsync(request.OrderId, ct);
        if (shipment is null)
            return Result.Fail<byte[]>(AdminErrors.NotFound("Shipment", request.OrderId));
        return Result.Ok(pdf.GenerateDispatchTicket(shipment));
    }
}

// --- Shipments & drivers ---

public record ListShipmentsAdminQuery(int Page, int PageSize) : IRequest<Result<IReadOnlyList<ShipmentSummaryDto>>>;

public class ListShipmentsAdminQueryHandler(IShipmentRepository shipments)
    : IRequestHandler<ListShipmentsAdminQuery, Result<IReadOnlyList<ShipmentSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<ShipmentSummaryDto>>> Handle(ListShipmentsAdminQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var list = await shipments.ListAsync(page, pageSize, ct);
        return Result.Ok((IReadOnlyList<ShipmentSummaryDto>)list.Select(s => new ShipmentSummaryDto(
            s.Id, s.OrderId, s.Status.ToString(), s.TrackingNumber,
            s.Driver?.Name, s.CreatedAt)).ToList());
    }
}

public record CreateShipmentCommand(Guid OrderId, Guid DriverId, string? TrackingNumber)
    : IRequest<Result<ShipmentDto>>;

public class CreateShipmentCommandHandler(IOrderRepository orders, IShipmentRepository shipments)
    : IRequestHandler<CreateShipmentCommand, Result<ShipmentDto>>
{
    public async Task<Result<ShipmentDto>> Handle(CreateShipmentCommand request, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(request.OrderId, ct);
        if (order is null)
            return Result.Fail<ShipmentDto>(AdminErrors.NotFound("Order", request.OrderId));
        if (order.Status != OrderStatus.ReadyToDispatch)
            return Result.Fail<ShipmentDto>(AdminErrors.InvalidState("La orden no está lista para despacho"));

        var shipment = new Shipment
        {
            OrderId = order.Id,
            DriverId = request.DriverId,
            Status = ShipmentStatus.Pending,
            TrackingNumber = request.TrackingNumber,
            ShippedAt = DateTime.UtcNow
        };
        var ticket = new DispatchTicket { TicketNumber = $"TKT-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}" };
        await shipments.CreateAsync(shipment, ticket, ct);
        await orders.UpdateStatusAsync(order.Id, OrderStatus.Dispatched, ct);

        return Result.Ok(new ShipmentDto(shipment.Id, shipment.OrderId, shipment.Status.ToString(),
            shipment.TrackingNumber, shipment.DriverId, ticket.TicketNumber));
    }
}

public record GenerateShipmentTicketPdfQuery(Guid ShipmentId) : IRequest<Result<byte[]>>;

public class GenerateShipmentTicketPdfQueryHandler(IShipmentRepository shipments, IPdfTicketGenerator pdf)
    : IRequestHandler<GenerateShipmentTicketPdfQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(GenerateShipmentTicketPdfQuery request, CancellationToken ct)
    {
        var shipment = await shipments.GetByIdAsync(request.ShipmentId, ct);
        if (shipment is null)
            return Result.Fail<byte[]>(AdminErrors.NotFound("Shipment", request.ShipmentId));
        return Result.Ok(pdf.GenerateDispatchTicket(shipment));
    }
}

public record MarkShipmentInTransitCommand(Guid ShipmentId) : IRequest<Result>;

public class MarkShipmentInTransitCommandHandler(IShipmentRepository shipments)
    : IRequestHandler<MarkShipmentInTransitCommand, Result>
{
    public async Task<Result> Handle(MarkShipmentInTransitCommand request, CancellationToken ct)
    {
        await shipments.UpdateStatusAsync(request.ShipmentId, ShipmentStatus.InTransit, ct);
        return Result.Ok();
    }
}

public record MarkShipmentDeliveredCommand(Guid ShipmentId) : IRequest<Result>;

public class MarkShipmentDeliveredCommandHandler(IShipmentRepository shipments)
    : IRequestHandler<MarkShipmentDeliveredCommand, Result>
{
    public async Task<Result> Handle(MarkShipmentDeliveredCommand request, CancellationToken ct)
    {
        await shipments.UpdateStatusAsync(request.ShipmentId, ShipmentStatus.Delivered, ct);
        return Result.Ok();
    }
}

public record ListDriversQuery : IRequest<Result<IReadOnlyList<DriverDto>>>;

public class ListDriversQueryHandler(IShipmentRepository shipments)
    : IRequestHandler<ListDriversQuery, Result<IReadOnlyList<DriverDto>>>
{
    public async Task<Result<IReadOnlyList<DriverDto>>> Handle(ListDriversQuery request, CancellationToken ct)
    {
        var drivers = await shipments.ListDriversAsync(ct);
        return Result.Ok((IReadOnlyList<DriverDto>)drivers.Select(d => new DriverDto(d.Id, d.Name, d.Phone, d.IsActive)).ToList());
    }
}

public record SaveDriverCommand(Guid? Id, string Name, string Phone, bool IsActive) : IRequest<Result<DriverDto>>;

public class SaveDriverCommandHandler(IShipmentRepository shipments)
    : IRequestHandler<SaveDriverCommand, Result<DriverDto>>
{
    public async Task<Result<DriverDto>> Handle(SaveDriverCommand request, CancellationToken ct)
    {
        var saved = await shipments.SaveDriverAsync(new Domain.Entities.Driver
        {
            Id = request.Id ?? Guid.Empty,
            Name = request.Name,
            Phone = request.Phone,
            IsActive = request.IsActive
        }, ct);
        return Result.Ok(new DriverDto(saved.Id, saved.Name, saved.Phone, saved.IsActive));
    }
}

public record DeleteDriverCommand(Guid Id) : IRequest<Result>;

public class DeleteDriverCommandHandler(IShipmentRepository shipments) : IRequestHandler<DeleteDriverCommand, Result>
{
    public async Task<Result> Handle(DeleteDriverCommand request, CancellationToken ct)
    {
        await shipments.DeleteDriverAsync(request.Id, ct);
        return Result.Ok();
    }
}

// --- Product options ---

public record ListProductOptionsQuery(Guid ProductId) : IRequest<Result<IReadOnlyList<ProductOptionDto>>>;

public class ListProductOptionsQueryHandler(IProductOptionRepository repo, IAdminCatalogRepository products)
    : IRequestHandler<ListProductOptionsQuery, Result<IReadOnlyList<ProductOptionDto>>>
{
    public async Task<Result<IReadOnlyList<ProductOptionDto>>> Handle(ListProductOptionsQuery request, CancellationToken ct)
    {
        if (await products.GetProductAsync(request.ProductId, ct) is null)
            return Result.Fail<IReadOnlyList<ProductOptionDto>>(AdminErrors.NotFound("Product", request.ProductId));
        var options = await repo.ListByProductAsync(request.ProductId, ct);
        return Result.Ok((IReadOnlyList<ProductOptionDto>)options.Select(AdminProductOptionMapping.Map).ToList());
    }
}

public record SaveProductOptionCommand(Guid ProductId, Guid? OptionId, string Name, int OptionType, int SortOrder)
    : IRequest<Result<ProductOptionDto>>;

public class SaveProductOptionCommandHandler(IProductOptionRepository repo, IAdminCatalogRepository products)
    : IRequestHandler<SaveProductOptionCommand, Result<ProductOptionDto>>
{
    public async Task<Result<ProductOptionDto>> Handle(SaveProductOptionCommand request, CancellationToken ct)
    {
        if (await products.GetProductAsync(request.ProductId, ct) is null)
            return Result.Fail<ProductOptionDto>(AdminErrors.NotFound("Product", request.ProductId));
        var saved = await repo.SaveOptionAsync(new ProductOption
        {
            Id = request.OptionId ?? Guid.Empty,
            ProductId = request.ProductId,
            Name = request.Name,
            OptionType = request.OptionType is 1 or 2 ? request.OptionType : 1,
            SortOrder = request.SortOrder
        }, ct);
        return Result.Ok(new ProductOptionDto(saved.Id, saved.ProductId, saved.Name, saved.OptionType, saved.SortOrder, []));
    }
}

public record DeleteProductOptionCommand(Guid ProductId, Guid OptionId) : IRequest<Result>;

public class DeleteProductOptionCommandHandler(IProductOptionRepository repo)
    : IRequestHandler<DeleteProductOptionCommand, Result>
{
    public async Task<Result> Handle(DeleteProductOptionCommand request, CancellationToken ct)
    {
        await repo.DeleteOptionAsync(request.OptionId, request.ProductId, ct);
        return Result.Ok();
    }
}

public record SaveOptionValueCommand(Guid ProductId, Guid OptionId, Guid? ValueId, string Value, int SortOrder)
    : IRequest<Result<OptionValueDto>>;

public class SaveOptionValueCommandHandler(IProductOptionRepository repo)
    : IRequestHandler<SaveOptionValueCommand, Result<OptionValueDto>>
{
    public async Task<Result<OptionValueDto>> Handle(SaveOptionValueCommand request, CancellationToken ct)
    {
        if (await repo.GetAsync(request.OptionId, request.ProductId, ct) is null)
            return Result.Fail<OptionValueDto>(AdminErrors.NotFound("ProductOption", request.OptionId));
        var saved = await repo.SaveValueAsync(new OptionValue
        {
            Id = request.ValueId ?? Guid.Empty,
            ProductOptionId = request.OptionId,
            Value = request.Value,
            SortOrder = request.SortOrder
        }, ct);
        return Result.Ok(new OptionValueDto(saved.Id, saved.Value, saved.SortOrder));
    }
}

public record DeleteOptionValueCommand(Guid ProductId, Guid OptionId, Guid ValueId) : IRequest<Result>;

public class DeleteOptionValueCommandHandler(IProductOptionRepository repo)
    : IRequestHandler<DeleteOptionValueCommand, Result>
{
    public async Task<Result> Handle(DeleteOptionValueCommand request, CancellationToken ct)
    {
        await repo.DeleteValueAsync(request.ValueId, request.OptionId, ct);
        return Result.Ok();
    }
}

internal static class AdminProductOptionMapping
{
    public static ProductOptionDto Map(ProductOption o) => new(
        o.Id, o.ProductId, o.Name, o.OptionType, o.SortOrder,
        o.Values.OrderBy(v => v.SortOrder).Select(v => new OptionValueDto(v.Id, v.Value, v.SortOrder)).ToList());
}
