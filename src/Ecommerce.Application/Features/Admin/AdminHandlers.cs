// Handlers admin: dashboard, covers, catálogo CRUD, inventario, pedidos, envíos, opciones de producto.
using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
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
                // Ya hay 5 activas: la nueva (o reactivada) queda inactiva sin slot principal
                wantActive = false;
                sortOrder = 0;
            }
            else if (existing is not null && existing.SortOrder is >= 1 and <= CoverRules.MaxPrincipalActive)
            {
                sortOrder = existing.SortOrder;
            }
            else
            {
                var next = await repo.GetNextPrincipalOrderAsync(ct);
                sortOrder = next ?? 0;
                if (sortOrder == 0)
                    wantActive = false;
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
        if (!wantActive && entity.SortOrder == 0)
            await repo.CompactPrincipalOrderAsync(ct);

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
        await repo.CompactPrincipalOrderAsync(ct);
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
            await repo.CompactPrincipalOrderAsync(ct);
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

public record ListFamiliesPagedAdminQuery(AdminTableQueryParams Query) : IRequest<Result<PagedFamiliesAdminDto>>;

public class ListFamiliesPagedAdminQueryHandler(IAdminCatalogRepository repo)
    : IRequestHandler<ListFamiliesPagedAdminQuery, Result<PagedFamiliesAdminDto>>
{
    public async Task<Result<PagedFamiliesAdminDto>> Handle(ListFamiliesPagedAdminQuery request, CancellationToken ct)
    {
        var q = request.Query;
        var (items, total) = await repo.ListFamiliesPagedAsync(q, ct);
        return Result.Ok(new PagedFamiliesAdminDto(
            items.Select(f => new FamilyAdminDto(f.Id, f.Name, f.Slug, f.SortOrder, f.IsActive)).ToList(),
            total,
            Math.Max(1, q.Page),
            Math.Clamp(q.PageSize, 1, 100)));
    }
}

public record ListCategoriesPagedAdminQuery(AdminTableQueryParams Query) : IRequest<Result<PagedCategoriesAdminDto>>;

public class ListCategoriesPagedAdminQueryHandler(IAdminCatalogRepository repo)
    : IRequestHandler<ListCategoriesPagedAdminQuery, Result<PagedCategoriesAdminDto>>
{
    public async Task<Result<PagedCategoriesAdminDto>> Handle(ListCategoriesPagedAdminQuery request, CancellationToken ct)
    {
        var q = request.Query;
        var (items, total) = await repo.ListCategoriesPagedAsync(q, ct);
        return Result.Ok(new PagedCategoriesAdminDto(items, total, Math.Max(1, q.Page), Math.Clamp(q.PageSize, 1, 100)));
    }
}

public record ListSubcategoriesPagedAdminQuery(AdminTableQueryParams Query) : IRequest<Result<PagedSubcategoriesAdminDto>>;

public class ListSubcategoriesPagedAdminQueryHandler(IAdminCatalogRepository repo)
    : IRequestHandler<ListSubcategoriesPagedAdminQuery, Result<PagedSubcategoriesAdminDto>>
{
    public async Task<Result<PagedSubcategoriesAdminDto>> Handle(ListSubcategoriesPagedAdminQuery request, CancellationToken ct)
    {
        var q = request.Query;
        var (items, total) = await repo.ListSubcategoriesPagedAsync(q, ct);
        return Result.Ok(new PagedSubcategoriesAdminDto(items, total, Math.Max(1, q.Page), Math.Clamp(q.PageSize, 1, 100)));
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

public record GetAdminOrderQuery(Guid OrderId) : IRequest<Result<OrderDetailDto>>;

public class GetAdminOrderQueryHandler(IOrderRepository orders, IDispatchRepository dispatch)
    : IRequestHandler<GetAdminOrderQuery, Result<OrderDetailDto>>
{
    public async Task<Result<OrderDetailDto>> Handle(GetAdminOrderQuery request, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(request.OrderId, ct);
        if (order is null)
            return Result.Fail<OrderDetailDto>(AdminErrors.NotFound("Order", request.OrderId));
        var dispatchInfo = await dispatch.GetOrderDispatchInfoAsync(request.OrderId, ct);
        return Result.Ok(OrderMapping.ToDetail(order, dispatch: dispatchInfo));
    }
}

public record MarkOrderReadyToDispatchCommand(Guid OrderId) : IRequest<Result>;

public class MarkOrderReadyToDispatchCommandHandler(IOrderRepository orders, IDispatchRepository dispatch)
    : IRequestHandler<MarkOrderReadyToDispatchCommand, Result>
{
    public async Task<Result> Handle(MarkOrderReadyToDispatchCommand request, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(request.OrderId, ct);
        if (order is null)
            return Result.Fail(AdminErrors.NotFound("Order", request.OrderId));
        if (order.Status != OrderStatus.Paid)
            return Result.Fail(AdminErrors.InvalidState("La orden debe estar pagada"));

        var locked = order.DispatchStatus is DispatchStatus.Batched or DispatchStatus.Routed
            or DispatchStatus.Assigned or DispatchStatus.InTransit or DispatchStatus.Delivered;
        if (locked)
            return Result.Fail(AdminErrors.InvalidState("El pedido ya está en proceso de despacho por lotes/rutas."));

        await dispatch.MarkOrderDispatchReadyAsync(order, ct);
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

public record SaveDriverCommand(
    Guid? Id,
    string Name,
    string Phone,
    string? Email,
    string? LicenseNumber,
    string? VehicleType,
    string? VehiclePlate,
    string? Notes,
    bool IsActive,
    bool CreateLoginAccount) : IRequest<Result<DriverDto>>;

public class SaveDriverCommandHandler(IShipmentRepository shipments, IUserRepository users)
    : IRequestHandler<SaveDriverCommand, Result<DriverDto>>
{
    public async Task<Result<DriverDto>> Handle(SaveDriverCommand request, CancellationToken ct)
    {
        Domain.Entities.Driver? existing = null;
        if (request.Id is { } editId && editId != Guid.Empty)
        {
            existing = await shipments.GetDriverWithUserAsync(editId, ct);
            if (existing is null)
                return Result.Fail<DriverDto>(AdminErrors.NotFound("Driver", editId));
        }

        var driver = new Domain.Entities.Driver
        {
            Id = request.Id ?? Guid.Empty,
            UserId = existing?.UserId,
            Name = request.Name.Trim(),
            Phone = request.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            LicenseNumber = string.IsNullOrWhiteSpace(request.LicenseNumber) ? null : request.LicenseNumber.Trim(),
            VehicleType = string.IsNullOrWhiteSpace(request.VehicleType) ? null : request.VehicleType.Trim(),
            VehiclePlate = string.IsNullOrWhiteSpace(request.VehiclePlate) ? null : request.VehiclePlate.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            IsActive = request.IsActive,
        };

        var saved = await shipments.SaveDriverAsync(driver, ct);

        var isNew = existing is null;
        var shouldProvision = !string.IsNullOrWhiteSpace(saved.Email)
            && (isNew || request.CreateLoginAccount || existing!.UserId is null);

        string? generatedTemp = null;
        if (shouldProvision)
        {
            var provision = await DriverAccountProvisioning.EnsureLoginAccountAsync(
                saved,
                saved.Email!,
                users,
                shipments,
                ct);
            if (provision.IsFailed)
                return Result.Fail<DriverDto>(provision.Errors);

            generatedTemp = provision.Value;
            saved = (await shipments.GetDriverWithUserAsync(saved.Id, ct))!;
        }

        return Result.Ok(AdminDriverMapping.Map(saved, generatedTemp));
    }
}

public record SetDriverTemporaryPasswordCommand(Guid DriverId) : IRequest<Result<DriverAccessCredentialsDto>>;

public class SetDriverTemporaryPasswordCommandHandler(IShipmentRepository shipments, IUserRepository users)
    : IRequestHandler<SetDriverTemporaryPasswordCommand, Result<DriverAccessCredentialsDto>>
{
    public async Task<Result<DriverAccessCredentialsDto>> Handle(SetDriverTemporaryPasswordCommand request, CancellationToken ct)
    {
        var driver = await shipments.GetDriverWithUserAsync(request.DriverId, ct);
        if (driver is null)
            return Result.Fail<DriverAccessCredentialsDto>(AdminErrors.NotFound("Driver", request.DriverId));

        if (string.IsNullOrWhiteSpace(driver.Email))
            return Result.Fail<DriverAccessCredentialsDto>(AdminErrors.InvalidState("El conductor debe tener email para acceso a la app."));

        var provision = await DriverAccountProvisioning.EnsureLoginAccountAsync(
            driver,
            driver.Email,
            users,
            shipments,
            ct);

        return provision.IsFailed
            ? Result.Fail<DriverAccessCredentialsDto>(provision.Errors)
            : Result.Ok(new DriverAccessCredentialsDto(provision.Value));
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

// --- Global product options (catálogo Laravel) ---

public record ListGlobalOptionsQuery() : IRequest<Result<IReadOnlyList<ProductOptionDto>>>;

public class ListGlobalOptionsQueryHandler(IProductOptionRepository repo)
    : IRequestHandler<ListGlobalOptionsQuery, Result<IReadOnlyList<ProductOptionDto>>>
{
    public async Task<Result<IReadOnlyList<ProductOptionDto>>> Handle(ListGlobalOptionsQuery request, CancellationToken ct)
    {
        var options = await repo.ListAllAsync(ct);
        return Result.Ok((IReadOnlyList<ProductOptionDto>)options.Select(AdminProductOptionMapping.Map).ToList());
    }
}

public record SaveGlobalOptionCommand(Guid? OptionId, string Name, int OptionType, int SortOrder)
    : IRequest<Result<ProductOptionDto>>;

public class SaveGlobalOptionCommandHandler(IProductOptionRepository repo)
    : IRequestHandler<SaveGlobalOptionCommand, Result<ProductOptionDto>>
{
    public async Task<Result<ProductOptionDto>> Handle(SaveGlobalOptionCommand request, CancellationToken ct)
    {
        ProductOption saved;
        if (request.OptionId is { } id)
        {
            var existing = await repo.GetByIdAsync(id, ct);
            if (existing is null)
                return Result.Fail<ProductOptionDto>(AdminErrors.NotFound("ProductOption", id));
            existing.Name = request.Name;
            existing.OptionType = request.OptionType is 1 or 2 ? request.OptionType : 1;
            existing.SortOrder = request.SortOrder;
            saved = await repo.SaveOptionAsync(existing, ct);
        }
        else
        {
            saved = await repo.SaveOptionAsync(new ProductOption
            {
                Name = request.Name,
                OptionType = request.OptionType is 1 or 2 ? request.OptionType : 1,
                SortOrder = request.SortOrder,
            }, ct);
        }

        var reloaded = await repo.GetByIdAsync(saved.Id, ct) ?? saved;
        return Result.Ok(AdminProductOptionMapping.Map(reloaded));
    }
}

public record DeleteGlobalOptionCommand(Guid OptionId) : IRequest<Result>;

public class DeleteGlobalOptionCommandHandler(IProductOptionRepository repo)
    : IRequestHandler<DeleteGlobalOptionCommand, Result>
{
    public async Task<Result> Handle(DeleteGlobalOptionCommand request, CancellationToken ct)
    {
        if (await repo.GetByIdAsync(request.OptionId, ct) is null)
            return Result.Fail(AdminErrors.NotFound("ProductOption", request.OptionId));
        if (await repo.HasAssignmentsAsync(request.OptionId, ct))
            return Result.Fail(AdminErrors.Conflict("Esta opción está asociada a uno o más productos."));
        await repo.DeleteOptionAsync(request.OptionId, ct);
        return Result.Ok();
    }
}

public record SaveGlobalOptionValueCommand(Guid OptionId, Guid? ValueId, string Value, string? Description, int SortOrder)
    : IRequest<Result<OptionValueDto>>;

public class SaveGlobalOptionValueCommandHandler(IProductOptionRepository repo)
    : IRequestHandler<SaveGlobalOptionValueCommand, Result<OptionValueDto>>
{
    public async Task<Result<OptionValueDto>> Handle(SaveGlobalOptionValueCommand request, CancellationToken ct)
    {
        if (await repo.GetByIdAsync(request.OptionId, ct) is null)
            return Result.Fail<OptionValueDto>(AdminErrors.NotFound("ProductOption", request.OptionId));

        var saved = await repo.SaveValueAsync(new OptionValue
        {
            Id = request.ValueId ?? Guid.Empty,
            ProductOptionId = request.OptionId,
            Value = request.Value,
            Description = request.Description,
            SortOrder = request.SortOrder,
        }, ct);
        return Result.Ok(new OptionValueDto(saved.Id, saved.Value, saved.Description, saved.SortOrder));
    }
}

public record DeleteGlobalOptionValueCommand(Guid OptionId, Guid ValueId) : IRequest<Result>;

public class DeleteGlobalOptionValueCommandHandler(IProductOptionRepository repo)
    : IRequestHandler<DeleteGlobalOptionValueCommand, Result>
{
    public async Task<Result> Handle(DeleteGlobalOptionValueCommand request, CancellationToken ct)
    {
        var option = await repo.GetByIdAsync(request.OptionId, ct);
        if (option is null)
            return Result.Fail(AdminErrors.NotFound("ProductOption", request.OptionId));
        if (option.Values.Count <= 1)
            return Result.Fail(AdminErrors.Conflict("La opción debe tener al menos un valor."));
        await repo.DeleteValueAsync(request.ValueId, request.OptionId, ct);
        return Result.Ok();
    }
}

// --- Product option assignments + variant generation ---

public record ListProductOptionAssignmentsQuery(Guid ProductId) : IRequest<Result<IReadOnlyList<ProductOptionAssignmentDto>>>;

public class ListProductOptionAssignmentsQueryHandler(IProductOptionRepository repo, IAdminCatalogRepository products)
    : IRequestHandler<ListProductOptionAssignmentsQuery, Result<IReadOnlyList<ProductOptionAssignmentDto>>>
{
    public async Task<Result<IReadOnlyList<ProductOptionAssignmentDto>>> Handle(ListProductOptionAssignmentsQuery request, CancellationToken ct)
    {
        if (await products.GetProductAsync(request.ProductId, ct) is null)
            return Result.Fail<IReadOnlyList<ProductOptionAssignmentDto>>(AdminErrors.NotFound("Product", request.ProductId));

        var assignments = await repo.ListAssignmentsAsync(request.ProductId, ct);
        var dtos = assignments.Select(AdminProductOptionMapping.MapAssignment).ToList();
        return Result.Ok((IReadOnlyList<ProductOptionAssignmentDto>)dtos);
    }
}

public record AttachProductOptionCommand(Guid ProductId, Guid OptionId, IReadOnlyList<Guid> ValueIds)
    : IRequest<Result<GenerateVariantsResultDto>>;

public class AttachProductOptionCommandHandler(
    IProductOptionRepository repo,
    IAdminCatalogRepository products)
    : IRequestHandler<AttachProductOptionCommand, Result<GenerateVariantsResultDto>>
{
    public async Task<Result<GenerateVariantsResultDto>> Handle(AttachProductOptionCommand request, CancellationToken ct)
    {
        if (await products.GetProductAsync(request.ProductId, ct) is null)
            return Result.Fail<GenerateVariantsResultDto>(AdminErrors.NotFound("Product", request.ProductId));
        if (await repo.GetByIdAsync(request.OptionId, ct) is null)
            return Result.Fail<GenerateVariantsResultDto>(AdminErrors.NotFound("ProductOption", request.OptionId));
        if (request.ValueIds.Count == 0)
            return Result.Fail<GenerateVariantsResultDto>(AdminErrors.Validation("Selecciona al menos un valor."));

        var existing = await repo.ListAssignmentsAsync(request.ProductId, ct);
        if (existing.Any(a => a.ProductOptionId == request.OptionId))
            return Result.Fail<GenerateVariantsResultDto>(AdminErrors.Conflict("Esta opción ya está asignada al producto."));

        await repo.AttachOptionAsync(request.ProductId, request.OptionId, request.ValueIds, ct);
        var result = await repo.GenerateVariantsAsync(request.ProductId, ct);
        return Result.Ok(result);
    }
}

public record DetachProductOptionCommand(Guid ProductId, Guid OptionId) : IRequest<Result<GenerateVariantsResultDto>>;

public class DetachProductOptionCommandHandler(IProductOptionRepository repo, IAdminCatalogRepository products)
    : IRequestHandler<DetachProductOptionCommand, Result<GenerateVariantsResultDto>>
{
    public async Task<Result<GenerateVariantsResultDto>> Handle(DetachProductOptionCommand request, CancellationToken ct)
    {
        if (await products.GetProductAsync(request.ProductId, ct) is null)
            return Result.Fail<GenerateVariantsResultDto>(AdminErrors.NotFound("Product", request.ProductId));

        await repo.DetachOptionAsync(request.ProductId, request.OptionId, ct);
        var result = await repo.GenerateVariantsAsync(request.ProductId, ct);
        return Result.Ok(result);
    }
}

public record GenerateProductVariantsCommand(Guid ProductId) : IRequest<Result<GenerateVariantsResultDto>>;

public class GenerateProductVariantsCommandHandler(IProductOptionRepository repo, IAdminCatalogRepository products)
    : IRequestHandler<GenerateProductVariantsCommand, Result<GenerateVariantsResultDto>>
{
    public async Task<Result<GenerateVariantsResultDto>> Handle(GenerateProductVariantsCommand request, CancellationToken ct)
    {
        if (await products.GetProductAsync(request.ProductId, ct) is null)
            return Result.Fail<GenerateVariantsResultDto>(AdminErrors.NotFound("Product", request.ProductId));

        var result = await repo.GenerateVariantsAsync(request.ProductId, ct);
        return Result.Ok(result);
    }
}

public record ListProductVariantsQuery(Guid ProductId) : IRequest<Result<IReadOnlyList<VariantAdminDto>>>;

public class ListProductVariantsQueryHandler(IProductOptionRepository repo, IAdminCatalogRepository products)
    : IRequestHandler<ListProductVariantsQuery, Result<IReadOnlyList<VariantAdminDto>>>
{
    public async Task<Result<IReadOnlyList<VariantAdminDto>>> Handle(ListProductVariantsQuery request, CancellationToken ct)
    {
        if (await products.GetProductAsync(request.ProductId, ct) is null)
            return Result.Fail<IReadOnlyList<VariantAdminDto>>(AdminErrors.NotFound("Product", request.ProductId));

        var variants = await repo.ListVariantsAsync(request.ProductId, ct);
        var dtos = variants.Select(v => new VariantAdminDto(
            v.Id, v.ProductId, v.Sku, v.Price, v.IsActive, v.Inventory?.QuantityOnHand ?? 0)).ToList();
        return Result.Ok((IReadOnlyList<VariantAdminDto>)dtos);
    }
}

internal static class AdminProductOptionMapping
{
    public static ProductOptionDto Map(ProductOption o) => new(
        o.Id, o.Name, o.OptionType, o.SortOrder,
        o.Values.OrderBy(v => v.SortOrder).Select(v => new OptionValueDto(v.Id, v.Value, v.Description, v.SortOrder)).ToList());

    public static ProductOptionAssignmentDto MapAssignment(ProductOptionAssignment a)
    {
        var selectedIds = OptionFeatureJson.Deserialize(a.FeaturesJson).Select(f => f.Id).ToHashSet();
        var selected = a.ProductOption.Values
            .Where(v => selectedIds.Contains(v.Id))
            .OrderBy(v => v.SortOrder)
            .Select(v => new OptionValueDto(v.Id, v.Value, v.Description, v.SortOrder))
            .ToList();
        return new ProductOptionAssignmentDto(
            a.ProductOptionId, a.ProductOption.Name, a.ProductOption.OptionType, selected);
    }
}
