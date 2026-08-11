using AssuranceService.Application.Common;
using AssuranceService.Infrastructure.Data;
using AssuranceService.Infrastructure.Messaging;
using AssuranceService.Infrastructure.Repositories;
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
        
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddDbContext<AssuranceDbContext>((sp, opt) =>
        {
            opt.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                sqlOptions.MigrationsAssembly(typeof(AssuranceDbContext).Assembly.GetName().Name);
            });
        });
        services.AddScoped<IAssuranceRepository, AssuranceRepository>();
        services.AddScoped<IPrimeRepository, PrimeRepository>();
        services.AddScoped<IGarantieRepository, GarantieRepository>();
        services.AddScoped<ITypeTransportRepository, TypeTransportRepository>();
        services.AddScoped<IDeviseRepository, DeviseRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IPortRepository, PortRepository>();
        services.AddScoped<ITransportDetailsRepository, TransportDetailsRepository>();
        services.AddScoped<IAvenantRepository, AvenantRepository>();
        services.AddScoped<IAvenantRegistrationService, AvenantRegistrationService>();

        var objectStorageProvider = config["ObjectStorage:Provider"];
        if (string.Equals(objectStorageProvider, "Local", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IObjectStorageService, LocalObjectStorageService>();
        else
            services.AddSingleton<IObjectStorageService, MinioObjectStorageService>();

        services.AddHttpClient();
        services.AddScoped<IPartenaireService, PartenaireService>();
        services.AddScoped<IDeclarationDossierClient, DeclarationDossierClient>();
        services.AddScoped<IDeclarationInvoiceImporter, DeclarationInvoiceImporter>();
        // Taux de change en local (table TauxDeChanges + config), plus d'appel au service externe
        services.AddScoped<ITauxChangeService, LocalTauxChangeService>();
        
        services.AddReferentielMessaging(config);
        services.AddMassTransitWithRabbitMq(config);

        return services;
    }
}
