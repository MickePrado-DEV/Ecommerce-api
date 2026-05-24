using Ecommerce.Api.Endpoints;
using Ecommerce.Application;
using Ecommerce.Application.Authorization;
using Ecommerce.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
    foreach (var perm in AdminPermissions.All)
        o.AddPolicy(perm, p => p.RequireClaim("permission", perm));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Ecommerce API"));
}

app.UseCors("Web");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

var api = app.MapGroup("/api/v1");
api.MapAuthEndpoints();
api.MapCatalogEndpoints();
api.MapCartEndpoints();

app.Run();