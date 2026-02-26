using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AssuranceService.Api.Extensions;

public static class ConsulExtensions
{
    public static IServiceCollection AddConsulServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var consulConfig = new ConsulConfig();
        configuration.GetSection("Consul").Bind(consulConfig);
        
        services.AddSingleton(consulConfig);
        services.AddSingleton<IConsulClient, ConsulClient>(p => 
            new ConsulClient(config => 
            {
                config.Address = new Uri($"http://{consulConfig.Host}:{consulConfig.Port}");
            }));
        
        return services;
    }

    public static IApplicationBuilder UseConsulServiceDiscovery(
        this IApplicationBuilder app)
    {
        var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
        var consulConfig = app.ApplicationServices.GetRequiredService<ConsulConfig>();
        var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
        var logger = app.ApplicationServices.GetRequiredService<ILogger<IConsulClient>>();

        // Enregistrement du service
        var registration = new AgentServiceRegistration
        {
            ID = consulConfig.ServiceId,
            Name = consulConfig.ServiceName,
            Address = consulConfig.ServiceAddress,
            Port = consulConfig.ServicePort,
            Tags = new[] { 
                "api", 
                "microservice", 
                "assurance", 
                "route-prefix:assurance", 
                "downstream-path:/api/v1/{everything}" 
            },
            Check = new AgentServiceCheck
            {
                HTTP = $"http://{consulConfig.ServiceAddress}:{consulConfig.ServicePort}{consulConfig.HealthCheckPath}",
                Interval = consulConfig.HealthCheckInterval,
                DeregisterCriticalServiceAfter = consulConfig.DeregisterAfter,
                Timeout = TimeSpan.FromSeconds(5)
            }
        };

        var consulUri = $"http://{consulConfig.Host}:{consulConfig.Port}";
        var healthCheckUrl = $"http://{consulConfig.ServiceAddress}:{consulConfig.ServicePort}{consulConfig.HealthCheckPath}";
        logger.LogInformation(
            "Enregistrement du service {ServiceName} auprès de Consul à {ConsulUri} (Health: {HealthUrl})...",
            consulConfig.ServiceName, consulUri, healthCheckUrl);

        try
        {
            // Désenregistrer si déjà existant
            consulClient.Agent.ServiceDeregister(registration.ID).Wait();
            
            // Enregistrer le service
            consulClient.Agent.ServiceRegister(registration).Wait();

            logger.LogInformation("Service {ServiceName} (ID: {ServiceId}) enregistré avec succès sur Consul.", 
                consulConfig.ServiceName,
                consulConfig.ServiceId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, 
                "Échec de l'enregistrement du service {ServiceName} auprès de Consul à {ConsulUri}. Vérifiez que le conteneur peut joindre Consul (depuis le conteneur, utiliser host.docker.internal si Consul est sur l'hôte).", 
                consulConfig.ServiceName, consulUri);
        }

        // Désenregistrement automatique à l'arrêt
        lifetime.ApplicationStopping.Register(() =>
        {
            logger.LogInformation("Désenregistrement du service {ServiceName} de Consul...", 
                consulConfig.ServiceName);
            try
            {
                consulClient.Agent.ServiceDeregister(registration.ID).Wait();
                logger.LogInformation("Service {ServiceName} désenregistré avec succès!", 
                    consulConfig.ServiceName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Échec du désenregistrement du service {ServiceName}", 
                    consulConfig.ServiceName);
            }
        });

        return app;
    }
}




