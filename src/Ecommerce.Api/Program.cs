// Punto de entrada de la API: configura servicios (DI), pipeline HTTP y rutas Minimal API.
using Ecommerce.Api.Endpoints;
using Ecommerce.Api.Middleware;
using Ecommerce.Application;
using Ecommerce.Application.Abstractions;
using Ecommerce.Infrastructure;
using Ecommerce.Infrastructure.Persistence.Sql;
using Ecommerce.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;

try
{
    // --- Fase 1: configuración (antes de escuchar peticiones) ---
    var builder = WebApplication.CreateBuilder(args);

    // Logs estructurados: consola + archivo rotativo en /logs
    builder.Host.UseSerilog((ctx, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console()
        .WriteTo.File(
            $"logs/ecommerce-{ctx.HostingEnvironment.EnvironmentName.ToLowerInvariant()}-.log",
            rollingInterval: RollingInterval.Day,
            shared: true));

    // Capa Application: MediatR, FluentValidation, ValidationBehavior
    builder.Services.AddApplication();
    // Capa Infrastructure: EF Core, repositorios, JWT, PDF
    builder.Services.AddInfrastructure(builder.Configuration);
    var uploadsRoot = Path.Combine(builder.Environment.ContentRootPath, "uploads");
    builder.Services.AddSingleton<ICoverImageStorage>(_ => new CoverImageStorage(uploadsRoot));
    builder.Services.AddOpenApi();

    // Permite llamadas desde el frontend (orígenes en appsettings → Cors:Origins)
    builder.Services.AddCors(o => o.AddPolicy("Web", p =>
        p.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>()!)
         .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

    // Autenticación JWT: valida el Bearer token en cada petición protegida
    var jwt = builder.Configuration.GetSection("Jwt");
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!))
            };
        });

    // Una política por permiso admin; los endpoints usan .RequireAuthorization(permiso)
    builder.Services.AddAuthorization(o =>
    {
        foreach (var perm in Ecommerce.Application.Authorization.AdminPermissions.All)
            o.AddPolicy(perm, p => p.RequireClaim("permission", perm));
    });

    // --- Fase 2: construir app y pipeline HTTP ---
    var app = builder.Build();

    // Crea tablas si no existen, corrige esquema viejo y ejecuta seed (usuarios demo)
    await DatabaseBootstrap.InitializeAsync(app.Services);

    // Captura excepciones no controladas y devuelve JSON con código HTTP adecuado
    app.UseMiddleware<ExceptionMiddleware>();

    // Documentación interactiva (solo fuera de producción)
    if (!app.Environment.IsProduction())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("Ecommerce API");
            options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
        });
    }

    // Archivos subidos (portadas, etc.) servidos desde /uploads
    Directory.CreateDirectory(uploadsRoot);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsRoot),
        RequestPath = "/uploads",
    });

    // Orden del pipeline: logging → CORS → identidad (JWT) → autorización (permisos)
    app.UseSerilogRequestLogging();
    app.UseCors("Web");
    app.UseAuthentication();
    app.UseAuthorization();

    // Health checks sin prefijo /api/v1
    app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

    app.MapGet("/ready", async (EcommerceDbContext db) =>
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503);
    });

    // Todas las rutas de negocio bajo /api/v1
    var api = app.MapGroup("/api/v1");
    api.MapAuthEndpoints();
    api.MapCatalogEndpoints();
    api.MapCartEndpoints();
    api.MapAddressEndpoints();
    api.MapCheckoutEndpoints();
    api.MapWishlistEndpoints();
    api.MapOrderEndpoints();
    api.MapAdminEndpoints();
    api.MapDriverEndpoints();

    await app.RunAsync();
}
catch (Exception ex)
{
    Serilog.Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Serilog.Log.CloseAndFlushAsync();
}

// Necesario para tests de integración (WebApplicationFactory)
public partial class Program;
