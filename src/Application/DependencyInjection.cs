using AssuranceService.Application.Common;
using AssuranceService.Application.Common.Behaviors;
using AssuranceService.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AssuranceService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        // Services métier
        services.AddScoped<INumeroGeneratorService, NumeroGeneratorService>();
        services.AddScoped<IPrimeCalculatorService, PrimeCalculatorService>();
        
        return services;
    }
}
