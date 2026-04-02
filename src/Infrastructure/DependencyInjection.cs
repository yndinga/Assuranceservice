using AssuranceService.Application.Common;
using AssuranceService.Infrastructure.Data;
using AssuranceService.Infrastructure.Repositories;
using AssuranceService.Infrastructure.Messaging;
using AssuranceService.Infrastructure.ExternalServices;
using AssuranceService.Infrastructure.Services;
using AssuranceService.Infrastructure.Storage;
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
        services.AddScoped<IAssuranceRepository, AssuranceRepository>();
        services.AddScoped<IPrimeRepository, PrimeRepository>();
        services.AddScoped<IGarantieRepository, GarantieRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IDeviseRepository, DeviseRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IPortRepository, PortRepository>();
        services.AddScoped<ITransportDetailsRepository, TransportDetailsRepository>();
        services.AddScoped<IAvenantRepository, AvenantRepository>();
        services.AddScoped<IAvenantRegistrationService, AvenantRegistrationService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Stockage objet (MinIO)
        services.AddSingleton<IObjectStorageService, MinioObjectStorageService>();

        // Services externes HTTP
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddScoped<IPartenaireService, PartenaireService>();
        // Taux de change en local (table TauxDeChanges + config), plus d'appel au service externe
        services.AddScoped<ITauxChangeService, LocalTauxChangeService>();
        
        // MassTransit
        services.AddMassTransitWithRabbitMq(config);
        
        return services;
    }
}
