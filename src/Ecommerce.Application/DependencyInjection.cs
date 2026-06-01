// Registro de servicios de la capa Application (sin EF ni HTTP).
using Ecommerce.Application.Common.Behaviors;
using Ecommerce.Application.Features.Auth.Validators;
using Ecommerce.Application.Features.Dispatch.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // MediatR: descubre todos los IRequestHandler en Features/* y registra ISender
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation: registra validadores de commands (ej. LoginCommandValidator)
        services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>();

        // Pipeline: valida cada command/query ANTES de ejecutar el handler
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<DispatchBatchService>();
        services.AddScoped<RoutePlannerService>();
        services.AddScoped<RouteAssignmentService>();
        services.AddScoped<RouteExecutionService>();

        return services;
    }
}
