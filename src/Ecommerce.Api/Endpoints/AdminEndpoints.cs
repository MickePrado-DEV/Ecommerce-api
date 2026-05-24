using Ecommerce.Api.Filters;
using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Authorization;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Application.DTOs.Shipments;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Api.Endpoints;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this RouteGroupBuilder group)
    {
        var admin = group.MapGroup("/admin").WithTags("Admin").RequireAuthorization();

        admin.MapGet("/dashboard", () => Results.Ok(new { message = "Admin dashboard" }))
            .RequireAuthorization(AdminPermissions.DashboardView);

        MapCatalogAdmin(admin);
        MapInventoryAdmin(admin);
        MapOrdersAdmin(admin);
        MapShipmentsAdmin(admin);

        return group;
    }

    private static void MapCatalogAdmin(RouteGroupBuilder admin)
    {
        var catalog = admin.MapGroup("/catalog");

        catalog.MapGet("/families", async (IAdminCatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListFamiliesAsync(ct)))
            .RequireAuthorization(AdminPermissions.FamiliesView);

        catalog.MapPost("/families", async (SaveFamilyRequest req, IAdminCatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.SaveFamilyAsync(req, ct)))
            .RequireAuthorization(AdminPermissions.FamiliesManage)
            .WithValidation<SaveFamilyRequest>();

        catalog.MapPut("/families/{id:guid}", async (Guid id, SaveFamilyRequest req, IAdminCatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.SaveFamilyAsync(req with { Id = id }, ct)))
            .RequireAuthorization(AdminPermissions.FamiliesManage)
            .WithValidation<SaveFamilyRequest>();

        catalog.MapDelete("/families/{id:guid}", async (Guid id, IAdminCatalogService svc, CancellationToken ct) =>
        {
            await svc.DeleteFamilyAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization(AdminPermissions.FamiliesManage);

        catalog.MapPost("/categories", async (SaveCategoryRequest req, IAdminCatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.SaveCategoryAsync(req, ct)))
            .RequireAuthorization(AdminPermissions.CategoriesManage)
            .WithValidation<SaveCategoryRequest>();

        catalog.MapPut("/categories/{id:guid}", async (Guid id, SaveCategoryRequest req, IAdminCatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.SaveCategoryAsync(req with { Id = id }, ct)))
            .RequireAuthorization(AdminPermissions.CategoriesManage)
            .WithValidation<SaveCategoryRequest>();

        catalog.MapDelete("/categories/{id:guid}", async (Guid id, IAdminCatalogService svc, CancellationToken ct) =>
        {
            await svc.DeleteCategoryAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization(AdminPermissions.CategoriesManage);

        catalog.MapPost("/subcategories", async (SaveSubcategoryRequest req, IAdminCatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.SaveSubcategoryAsync(req, ct)))
            .RequireAuthorization(AdminPermissions.SubcategoriesManage)
            .WithValidation<SaveSubcategoryRequest>();

        catalog.MapPut("/subcategories/{id:guid}", async (Guid id, SaveSubcategoryRequest req, IAdminCatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.SaveSubcategoryAsync(req with { Id = id }, ct)))
            .RequireAuthorization(AdminPermissions.SubcategoriesManage)
            .WithValidation<SaveSubcategoryRequest>();

        catalog.MapDelete("/subcategories/{id:guid}", async (Guid id, IAdminCatalogService svc, CancellationToken ct) =>
        {
            await svc.DeleteSubcategoryAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization(AdminPermissions.SubcategoriesManage);

        catalog.MapGet("/products", async (int page, int pageSize, IAdminCatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListProductsAsync(page, pageSize, ct)))
            .RequireAuthorization(AdminPermissions.ProductsView);

        catalog.MapPost("/products", async (SaveProductRequest req, IAdminCatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.SaveProductAsync(req, ct)))
            .RequireAuthorization(AdminPermissions.ProductsManage)
            .WithValidation<SaveProductRequest>();

        catalog.MapPut("/products/{id:guid}", async (Guid id, SaveProductRequest req, IAdminCatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.SaveProductAsync(req with { Id = id }, ct)))
            .RequireAuthorization(AdminPermissions.ProductsManage)
            .WithValidation<SaveProductRequest>();

        catalog.MapDelete("/products/{id:guid}", async (Guid id, IAdminCatalogService svc, CancellationToken ct) =>
        {
            await svc.DeleteProductAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization(AdminPermissions.ProductsManage);

        catalog.MapPost("/variants", async (SaveVariantRequest req, IAdminCatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.SaveVariantAsync(req, ct)))
            .RequireAuthorization(AdminPermissions.ProductsManage)
            .WithValidation<SaveVariantRequest>();

        catalog.MapPut("/variants/{id:guid}", async (Guid id, SaveVariantRequest req, IAdminCatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.SaveVariantAsync(req with { Id = id }, ct)))
            .RequireAuthorization(AdminPermissions.ProductsManage)
            .WithValidation<SaveVariantRequest>();

        catalog.MapDelete("/variants/{id:guid}", async (Guid id, IAdminCatalogService svc, CancellationToken ct) =>
        {
            await svc.DeleteVariantAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization(AdminPermissions.ProductsManage);
    }

    private static void MapInventoryAdmin(RouteGroupBuilder admin)
    {
        var stock = admin.MapGroup("/inventory");

        stock.MapGet("/", async (IInventoryService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListAsync(ct)))
            .RequireAuthorization(AdminPermissions.StockView);

        stock.MapPut("/{variantId:guid}", async (Guid variantId, SetInventoryRequest req, IInventoryService svc, CancellationToken ct) =>
            Results.Ok(await svc.SetStockAsync(variantId, req, ct)))
            .RequireAuthorization(AdminPermissions.StockManage)
            .WithValidation<SetInventoryRequest>();
    }

    private static void MapOrdersAdmin(RouteGroupBuilder admin)
    {
        var orders = admin.MapGroup("/orders");

        orders.MapGet("/", async (int page, int pageSize, string? status, IAdminOrderService svc, CancellationToken ct) =>
        {
            OrderStatus? st = Enum.TryParse<OrderStatus>(status, true, out var parsed) ? parsed : null;
            return Results.Ok(await svc.ListAsync(page, pageSize, st, ct));
        }).RequireAuthorization(AdminPermissions.OrdersView);

        orders.MapGet("/{orderId:guid}", async (Guid orderId, IAdminOrderService svc, CancellationToken ct) =>
        {
            var order = await svc.GetAsync(orderId, ct);
            return order is null ? Results.NotFound() : Results.Ok(order);
        }).RequireAuthorization(AdminPermissions.OrdersView);

        orders.MapPost("/{orderId:guid}/ready", async (Guid orderId, IAdminOrderService svc, CancellationToken ct) =>
        {
            await svc.MarkReadyToDispatchAsync(orderId, ct);
            return Results.NoContent();
        }).RequireAuthorization(AdminPermissions.OrdersManage);
    }

    private static void MapShipmentsAdmin(RouteGroupBuilder admin)
    {
        var shipments = admin.MapGroup("/shipments");

        shipments.MapPost("/", async (CreateShipmentRequest req, IAdminShipmentService svc, CancellationToken ct) =>
            Results.Ok(await svc.CreateShipmentAsync(req, ct)))
            .RequireAuthorization(AdminPermissions.ShipmentsManage)
            .WithValidation<CreateShipmentRequest>();

        shipments.MapGet("/{shipmentId:guid}/ticket.pdf", async (Guid shipmentId, IAdminShipmentService svc, CancellationToken ct) =>
        {
            var pdf = await svc.GenerateTicketPdfAsync(shipmentId, ct);
            return Results.File(pdf, "application/pdf", $"ticket-{shipmentId}.pdf");
        }).RequireAuthorization(AdminPermissions.ShipmentsView);

        var drivers = admin.MapGroup("/drivers");
        drivers.MapGet("/", async (IAdminShipmentService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListDriversAsync(ct)))
            .RequireAuthorization(AdminPermissions.DriversView);

        drivers.MapPost("/", async (SaveDriverRequest req, IAdminShipmentService svc, CancellationToken ct) =>
            Results.Ok(await svc.SaveDriverAsync(req, ct)))
            .RequireAuthorization(AdminPermissions.DriversManage)
            .WithValidation<SaveDriverRequest>();

        drivers.MapPut("/{id:guid}", async (Guid id, SaveDriverRequest req, IAdminShipmentService svc, CancellationToken ct) =>
            Results.Ok(await svc.SaveDriverAsync(req with { Id = id }, ct)))
            .RequireAuthorization(AdminPermissions.DriversManage)
            .WithValidation<SaveDriverRequest>();
    }
}
