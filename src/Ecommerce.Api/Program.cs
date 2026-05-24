using Ecommerce.Api.Endpoints;
using Ecommerce.Api.Middleware;
using Ecommerce.Application;
using Ecommerce.Infrastructure;
using Ecommerce.Infrastructure.Persistence.Sql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console()
        .WriteTo.File(
            $"logs/ecommerce-{ctx.HostingEnvironment.EnvironmentName.ToLowerInvariant()}-.log",
            rollingInterval: RollingInterval.Day,
            shared: true));

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddOpenApi();

    builder.Services.AddCors(o => o.AddPolicy("Web", p =>
        p.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>()!)
         .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

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

    builder.Services.AddAuthorization(o =>
    {
        foreach (var perm in Ecommerce.Application.Authorization.AdminPermissions.All)
            o.AddPolicy(perm, p => p.RequireClaim("permission", perm));
    });

    var app = builder.Build();

    await DatabaseBootstrap.InitializeAsync(app.Services);

    app.UseMiddleware<ExceptionMiddleware>();

    if (!app.Environment.IsProduction())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("Ecommerce API");
            options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
        });
    }

    app.UseSerilogRequestLogging();
    app.UseCors("Web");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

    app.MapGet("/ready", async (EcommerceDbContext db) =>
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503);
    });

    var api = app.MapGroup("/api/v1");
    api.MapAuthEndpoints();
    api.MapCatalogEndpoints();
    api.MapCartEndpoints();
    api.MapAddressEndpoints();
    api.MapCheckoutEndpoints();
    api.MapOrderEndpoints();
    api.MapAdminEndpoints();

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

public partial class Program;
