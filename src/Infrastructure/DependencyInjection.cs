using AssuranceService.Application.Common;
using AssuranceService.Infrastructure.Data;
using AssuranceService.Infrastructure.Repositories;
using AssuranceService.Infrastructure.Messaging;
using AssuranceService.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AssuranceService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("AssuranceConnection") 
                               ?? "Server=localhost, 1420;Database=MS_ASSURANCE;User Id=sa;Password=DevStrongPwd@123;TrustServerCertificate=True;";
        
        services.AddDbContext<AssuranceDbContext>(opt =>
        {
            opt.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                // Migrations dans Infrastructure ; au run depuis Api, EF les trouve ainsi (sinon "En attente : 0")
                sqlOptions.MigrationsAssembly(typeof(AssuranceDbContext).Assembly.GetName().Name);
            });
        });
        
        // Repositories
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IAssuranceRepository, AssuranceRepository>();
        services.AddScoped<IMarchandiseRepository, MarchandiseRepository>();
        services.AddScoped<IPrimeRepository, PrimeRepository>();
        services.AddScoped<IGarantieRepository, GarantieRepository>();
        services.AddScoped<IVoyageRepository, VoyageRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();

        // Services externes HTTP
        services.AddHttpClient();
        services.AddScoped<IPartenaireService, PartenaireService>();
        services.AddScoped<ITauxChangeService, TauxChangeService>();
        
        // MassTransit
        services.AddMassTransitWithRabbitMq(config);
        
        return services;
    }
}
