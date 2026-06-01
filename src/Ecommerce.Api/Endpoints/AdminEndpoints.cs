// Panel admin: dashboard, covers, catálogo, inventario, pedidos, envíos, conductores, opciones.
// Cada ruta exige JWT + permiso concreto (AdminPermissions.*).
using Ecommerce.Api.Extensions;
using static Ecommerce.Api.Extensions.AdminTableQueryBinding;
using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Authorization;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Application.DTOs.Shipments;
using Ecommerce.Application.DTOs.Dispatch;
using Ecommerce.Application.Features.Admin;
using Ecommerce.Application.Features.Dispatch;
using Ecommerce.Domain.Emums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Api.Endpoints;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this RouteGroupBuilder group)
    {
        var admin = group.MapGroup("/admin").WithTags("Admin").RequireAuthorization();

        admin.MapGet("/dashboard/stats", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetDashboardStatsQuery(), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DashboardView);

        admin.MapGet("/dashboard", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetDashboardStatsQuery(), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DashboardView);

        MapCoversAdmin(admin);
        MapCatalogAdmin(admin);
        MapCatalogAliases(admin);
        MapInventoryAdmin(admin);
        MapOrdersAdmin(admin);
        MapShipmentsAdmin(admin);
        MapDispatchAdmin(admin);
        MapProductOptionsAdmin(admin);
        MapUsersAdmin(admin);
        MapRolesAdmin(admin);

        return group;
    }

    // CRUD portadas del home + reordenar
    private static void MapCoversAdmin(RouteGroupBuilder admin)
    {
        var covers = admin.MapGroup("/covers");

        covers.MapGet("/", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListCoversAdminQuery(), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.CoversView);

        covers.MapGet("/paged", async (int page, int pageSize, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListCoversPagedAdminQuery(page, pageSize), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.CoversView);

        covers.MapPost("/upload", async (IFormFile file, ICoverImageStorage storage, CancellationToken ct) =>
        {
            await using var stream = file.OpenReadStream();
            var result = await storage.SaveAsync(stream, file.FileName, file.Length, ct);
            return result.IsSuccess
                ? Results.Ok(new CoverImageUploadDto(result.Value))
                : Results.BadRequest(new { message = result.Errors.FirstOrDefault()?.Message ?? "Error al subir." });
        })
            .RequireAuthorization(AdminPermissions.CoversManage)
            .DisableAntiforgery();

        covers.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetCoverAdminQuery(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.CoversView);

        covers.MapPost("/", async (SaveCoverRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveCoverCommand(req.Id, req.Title, req.ImageUrl, req.LinkUrl, req.IsActive, req.StartsAt, req.EndsAt), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.CoversManage);

        covers.MapPut("/{id:guid}", async (Guid id, SaveCoverRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveCoverCommand(id, req.Title, req.ImageUrl, req.LinkUrl, req.IsActive, req.StartsAt, req.EndsAt), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.CoversManage);

        covers.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteCoverCommand(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.CoversManage);

        covers.MapPatch("/reorder", async (ReorderCoversRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ReorderCoversCommand(req.Ids), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.CoversManage);
    }

    // CRUD familias, categorías, subcategorías, productos y variantes bajo /admin/catalog
    private static void MapCatalogAdmin(RouteGroupBuilder admin)
    {
        var catalog = admin.MapGroup("/catalog");

        catalog.MapGet("/families", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListFamiliesAdminQuery(), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.FamiliesView);

        catalog.MapGet("/families/paged", async (
            int page,
            int pageSize,
            string? sortKey,
            string? sortDir,
            string? search,
            string? initials,
            string? idBuckets,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new ListFamiliesPagedAdminQuery(
                new AdminTableQueryParams(
                    page, pageSize, sortKey, sortDir ?? "asc", search,
                    NameInitials: ParseStrings(initials),
                    IdBuckets: ParseStrings(idBuckets))), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.FamiliesView);

        catalog.MapGet("/categories/paged", async (
            int page,
            int pageSize,
            string? sortKey,
            string? sortDir,
            string? search,
            string? familyName,
            string? familyIds,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new ListCategoriesPagedAdminQuery(
                new AdminTableQueryParams(
                    page, pageSize, sortKey, sortDir ?? "asc", search, familyName,
                    FamilyIds: ParseGuids(familyIds))), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.CategoriesView);

        catalog.MapGet("/subcategories/paged", async (
            int page,
            int pageSize,
            string? sortKey,
            string? sortDir,
            string? search,
            string? categoryName,
            string? familyName,
            string? familyIds,
            string? categoryIds,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new ListSubcategoriesPagedAdminQuery(
                new AdminTableQueryParams(
                    page, pageSize, sortKey, sortDir ?? "asc", search, familyName, categoryName,
                    FamilyIds: ParseGuids(familyIds),
                    CategoryIds: ParseGuids(categoryIds))), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.SubcategoriesView);

        catalog.MapPost("/families", async (SaveFamilyRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveFamilyCommand(req.Id, req.Name, req.Slug, req.SortOrder, req.IsActive), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.FamiliesManage);

        catalog.MapPut("/families/{id:guid}", async (Guid id, SaveFamilyRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveFamilyCommand(id, req.Name, req.Slug, req.SortOrder, req.IsActive), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.FamiliesManage);

        catalog.MapDelete("/families/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteFamilyCommand(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.FamiliesManage);

        catalog.MapPost("/categories", async (SaveCategoryRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveCategoryCommand(req.Id, req.FamilyId, req.Name, req.Slug, req.SortOrder, req.IsActive), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.CategoriesManage);

        catalog.MapPut("/categories/{id:guid}", async (Guid id, SaveCategoryRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveCategoryCommand(id, req.FamilyId, req.Name, req.Slug, req.SortOrder, req.IsActive), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.CategoriesManage);

        catalog.MapDelete("/categories/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteCategoryCommand(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.CategoriesManage);

        catalog.MapPost("/subcategories", async (SaveSubcategoryRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveSubcategoryCommand(req.Id, req.CategoryId, req.Name, req.Slug, req.SortOrder, req.IsActive), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.SubcategoriesManage);

        catalog.MapPut("/subcategories/{id:guid}", async (Guid id, SaveSubcategoryRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveSubcategoryCommand(id, req.CategoryId, req.Name, req.Slug, req.SortOrder, req.IsActive), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.SubcategoriesManage);

        catalog.MapDelete("/subcategories/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteSubcategoryCommand(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.SubcategoriesManage);

        catalog.MapGet("/products", async (int page, int pageSize, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListProductsAdminQuery(page, pageSize), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.ProductsView);

        catalog.MapPost("/products", async (SaveProductRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveProductCommand(req.Id, req.SubcategoryId, req.Name, req.Slug, req.Description, req.BasePrice, req.IsActive), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.ProductsManage);

        catalog.MapPut("/products/{id:guid}", async (Guid id, SaveProductRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveProductCommand(id, req.SubcategoryId, req.Name, req.Slug, req.Description, req.BasePrice, req.IsActive), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.ProductsManage);

        catalog.MapDelete("/products/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteProductCommand(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.ProductsManage);

        catalog.MapPost("/variants", async (SaveVariantRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveVariantCommand(req.Id, req.ProductId, req.Sku, req.Price, req.IsActive, req.InitialStock), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.ProductsManage);

        catalog.MapPut("/variants/{id:guid}", async (Guid id, SaveVariantRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveVariantCommand(id, req.ProductId, req.Sku, req.Price, req.IsActive, req.InitialStock), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.ProductsManage);

        catalog.MapDelete("/variants/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteVariantCommand(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.ProductsManage);
    }

    // Rutas alias /admin/families y /admin/products (compatibilidad con cliente Laravel)
    private static void MapCatalogAliases(RouteGroupBuilder admin)
    {
        admin.MapGet("/families", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListFamiliesAdminQuery(), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.FamiliesView);

        admin.MapPost("/families", async (SaveFamilyRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveFamilyCommand(req.Id, req.Name, req.Slug, req.SortOrder, req.IsActive), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.FamiliesManage);

        admin.MapPut("/families/{id:guid}", async (Guid id, SaveFamilyRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveFamilyCommand(id, req.Name, req.Slug, req.SortOrder, req.IsActive), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.FamiliesManage);

        admin.MapDelete("/families/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteFamilyCommand(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.FamiliesManage);

        admin.MapGet("/products", async (int page, int pageSize, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListProductsAdminQuery(page, pageSize), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.ProductsView);
    }

    // Opciones globales + asignaciones por producto (flujo Laravel)
    private static void MapProductOptionsAdmin(RouteGroupBuilder admin)
    {
        admin.MapGet("/options", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListGlobalOptionsQuery(), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OptionsView);

        admin.MapPost("/options", async (SaveProductOptionRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveGlobalOptionCommand(null, req.Name, req.OptionType, req.SortOrder), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OptionsManage);

        admin.MapPut("/options/{optionId:guid}", async (Guid optionId, SaveProductOptionRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveGlobalOptionCommand(optionId, req.Name, req.OptionType, req.SortOrder), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OptionsManage);

        admin.MapDelete("/options/{optionId:guid}", async (Guid optionId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteGlobalOptionCommand(optionId), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OptionsManage);

        admin.MapPost("/options/{optionId:guid}/values", async (Guid optionId, SaveOptionValueRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveGlobalOptionValueCommand(optionId, null, req.Value, req.Description, req.SortOrder), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OptionsManage);

        admin.MapPut("/options/{optionId:guid}/values/{valueId:guid}", async (
            Guid optionId, Guid valueId, SaveOptionValueRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveGlobalOptionValueCommand(optionId, valueId, req.Value, req.Description, req.SortOrder), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OptionsManage);

        admin.MapDelete("/options/{optionId:guid}/values/{valueId:guid}", async (
            Guid optionId, Guid valueId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteGlobalOptionValueCommand(optionId, valueId), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OptionsManage);

        admin.MapGet("/products/{productId:guid}/option-assignments", async (Guid productId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListProductOptionAssignmentsQuery(productId), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OptionsView);

        admin.MapPost("/products/{productId:guid}/option-assignments", async (
            Guid productId, AttachProductOptionRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new AttachProductOptionCommand(productId, req.OptionId, req.ValueIds), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OptionsManage);

        admin.MapDelete("/products/{productId:guid}/option-assignments/{optionId:guid}", async (
            Guid productId, Guid optionId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DetachProductOptionCommand(productId, optionId), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OptionsManage);

        admin.MapPost("/products/{productId:guid}/variants/generate", async (Guid productId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GenerateProductVariantsCommand(productId), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OptionsManage);

        admin.MapGet("/products/{productId:guid}/variants", async (Guid productId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListProductVariantsQuery(productId), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OptionsView);

        admin.MapPut("/variants/{variantId:guid}", async (Guid variantId, SaveVariantRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveVariantCommand(variantId, req.ProductId, req.Sku, req.Price, req.IsActive, req.InitialStock), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.ProductsManage);
    }

    // Consulta y ajuste de stock por variante
    private static void MapInventoryAdmin(RouteGroupBuilder admin)
    {
        var stock = admin.MapGroup("/inventory");

        stock.MapGet("/", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListInventoryQuery(), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.StockView);

        stock.MapGet("/{variantId:guid}", async (Guid variantId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ListInventoryQuery(), ct);
            if (result.IsFailed) return result.ToHttpResult();
            var item = result.Value.FirstOrDefault(i => i.VariantId == variantId);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).RequireAuthorization(AdminPermissions.StockView);

        stock.MapPut("/{variantId:guid}", async (Guid variantId, SetInventoryRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SetInventoryCommand(variantId, req.QuantityOnHand), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.StockManage);

        stock.MapPatch("/{variantId:guid}", async (Guid variantId, SetInventoryRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SetInventoryCommand(variantId, req.QuantityOnHand), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.StockManage);
    }

    // Listado/detalle pedidos, marcar listo para despacho, ticket PDF por orden
    private static void MapOrdersAdmin(RouteGroupBuilder admin)
    {
        var orders = admin.MapGroup("/orders");

        orders.MapGet("/", async (int page, int pageSize, string? status, ISender sender, CancellationToken ct) =>
        {
            OrderStatus? st = Enum.TryParse<OrderStatus>(status, true, out var parsed) ? parsed : null;
            return (await sender.Send(new ListAdminOrdersQuery(page, pageSize, st), ct)).ToHttpResult();
        }).RequireAuthorization(AdminPermissions.OrdersView);

        orders.MapGet("/{orderId:guid}", async (Guid orderId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetAdminOrderQuery(orderId), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OrdersView);

        orders.MapGet("/{orderId:guid}/ticket", async (Guid orderId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GenerateOrderTicketPdfQuery(orderId), ct))
                .ToHttpResult(pdf => Results.File(pdf, "application/pdf", $"ticket-{orderId}.pdf")))
            .RequireAuthorization(AdminPermissions.OrdersView);

        orders.MapPatch("/{orderId:guid}/ready-to-dispatch", async (Guid orderId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new MarkOrderReadyToDispatchCommand(orderId), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OrdersManage);

        orders.MapPost("/{orderId:guid}/ready", async (Guid orderId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new MarkOrderReadyToDispatchCommand(orderId), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.OrdersManage);
    }

    // Envíos, estados in-transit/delivered, conductores, PDF ticket
    private static void MapShipmentsAdmin(RouteGroupBuilder admin)
    {
        var shipments = admin.MapGroup("/shipments");

        shipments.MapGet("/", async (int page, int pageSize, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListShipmentsAdminQuery(page, pageSize), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.ShipmentsView);

        shipments.MapPost("/", async (CreateShipmentRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CreateShipmentCommand(req.OrderId, req.DriverId, req.TrackingNumber), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.ShipmentsManage);

        shipments.MapGet("/{shipmentId:guid}/ticket.pdf", async (Guid shipmentId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GenerateShipmentTicketPdfQuery(shipmentId), ct))
                .ToHttpResult(pdf => Results.File(pdf, "application/pdf", $"ticket-{shipmentId}.pdf")))
            .RequireAuthorization(AdminPermissions.ShipmentsView);

        shipments.MapPatch("/{shipmentId:guid}/in-transit", async (Guid shipmentId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new MarkShipmentInTransitCommand(shipmentId), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.ShipmentsManage);

        shipments.MapPatch("/{shipmentId:guid}/delivered", async (Guid shipmentId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new MarkShipmentDeliveredCommand(shipmentId), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.ShipmentsManage);

        var drivers = admin.MapGroup("/drivers");
        drivers.MapGet("/", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListDriversQuery(), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DriversView);

        drivers.MapPost("/", async (SaveDriverRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveDriverCommand(
                req.Id, req.Name, req.Phone, req.Email, req.LicenseNumber,
                req.VehicleType, req.VehiclePlate, req.Notes, req.IsActive, req.CreateLoginAccount), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DriversManage);

        drivers.MapPut("/{id:guid}", async (Guid id, SaveDriverRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SaveDriverCommand(
                id, req.Name, req.Phone, req.Email, req.LicenseNumber,
                req.VehicleType, req.VehiclePlate, req.Notes, req.IsActive, req.CreateLoginAccount), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DriversManage);

        drivers.MapPut("/{id:guid}/temporary-password", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new SetDriverTemporaryPasswordCommand(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DriversManage);

        drivers.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteDriverCommand(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DriversManage);
    }

    private static void MapDispatchAdmin(RouteGroupBuilder admin)
    {
        var dispatch = admin.MapGroup("/dispatch");

        dispatch.MapGet("/settings", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetDispatchSettingsQuery(), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchView);

        dispatch.MapPost("/settings", async (UpdateDispatchSettingsRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateDispatchSettingsCommand(req), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchManage);

        dispatch.MapGet("/queue", async (
            int page,
            int pageSize,
            DateTime? from,
            DateTime? to,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetDispatchQueueQuery(new DispatchQueueFilter(from, to, page, pageSize)), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchView);

        dispatch.MapPost("/batches/auto", async (AutoCreateBatchesRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new AutoCreateDispatchBatchesCommand(req), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchManage);

        dispatch.MapPost("/batches/manual", async (ManualCreateBatchRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ManualCreateDispatchBatchCommand(req), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchManage);

        dispatch.MapGet("/batches", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListDispatchBatchesQuery(), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchView);

        dispatch.MapGet("/batches/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetDispatchBatchQuery(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchView);

        dispatch.MapPost("/batches/{id:guid}/routes", async (
            Guid id,
            CreateRouteFromBatchRequest req,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new CreateRouteFromBatchCommand(id, req), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchManage);

        dispatch.MapGet("/routes", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListDeliveryRoutesQuery(), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchView);

        dispatch.MapGet("/routes/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetDeliveryRouteQuery(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchView);

        dispatch.MapPost("/routes/{id:guid}/assign", async (
            Guid id,
            AssignRouteRequest req,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new AssignDeliveryRouteCommand(id, req), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchManage);

        dispatch.MapPost("/routes/{id:guid}/start", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new StartDeliveryRouteCommand(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchManage);

        dispatch.MapPost("/routes/{id:guid}/finish", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new FinishDeliveryRouteCommand(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchManage);

        dispatch.MapPost("/stops/{id:guid}/delivered", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new MarkStopDeliveredCommand(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchManage);

        dispatch.MapPost("/stops/{id:guid}/failed", async (
            Guid id,
            FailStopRequest? req,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new MarkStopFailedCommand(id, req), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.DispatchManage);
    }

    private static void MapUsersAdmin(RouteGroupBuilder admin)
    {
        var users = admin.MapGroup("/users");

        users.MapGet("/", async (int page, int pageSize, string? search, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListUsersAdminQuery(page, pageSize, search), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.UsersView);

        users.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetUserAdminQuery(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.UsersView);

        users.MapPost("/", async (CreateUserAdminRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CreateUserAdminCommand(
                req.Email, req.Password, req.FirstName, req.LastName, req.Phone, req.IsActive, req.RoleCodes), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.UsersManage);

        users.MapPut("/{id:guid}", async (Guid id, UpdateUserAdminRequest req, HttpContext ctx, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateUserAdminCommand(id, ctx.GetUserId(), req.IsActive, req.RoleCodes), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.UsersManage);
    }

    private static void MapRolesAdmin(RouteGroupBuilder admin)
    {
        var roles = admin.MapGroup("/roles");

        roles.MapGet("/", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListRolesAdminQuery(), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.RolesView);

        roles.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetRoleAdminQuery(id), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.RolesView);

        roles.MapPut("/{id:guid}/permissions", async (Guid id, UpdateRolePermissionsRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateRolePermissionsCommand(id, req.PermissionCodes), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.RolesManage);

        admin.MapGet("/permissions", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListPermissionsAdminQuery(), ct)).ToHttpResult())
            .RequireAuthorization(AdminPermissions.RolesView);
    }
}
