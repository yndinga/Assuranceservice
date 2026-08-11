using AssuranceService.Application.Common;
using AssuranceService.Infrastructure.Messaging.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AssuranceService.Infrastructure.Messaging;

public static class ReferentielMessagingRegistration
{
    public static IServiceCollection AddReferentielMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ReferentielMessagingOptions>(
            configuration.GetSection(ReferentielMessagingOptions.SectionName));
        services.PostConfigure<ReferentielMessagingOptions>(opts =>
        {
            var merged = ReferentielMessagingOptions.Bind(configuration);
            opts.RabbitMq = merged.RabbitMq;
            opts.ReferentielServiceBaseUrl = merged.ReferentielServiceBaseUrl;
            if (configuration.GetValue<bool?>($"{ReferentielMessagingOptions.SectionName}:Enabled") is bool enabled)
                opts.Enabled = enabled;
            if (configuration.GetValue<bool?>($"{ReferentielMessagingOptions.SectionName}:InitialSyncOnStartup") is bool sync)
                opts.InitialSyncOnStartup = sync;
        });

        services.AddScoped<IReferentielSyncService, ReferentielSyncService>();
        services.AddHttpClient(ReferentielInitialSyncHostedService.HttpClientName, c => c.Timeout = TimeSpan.FromMinutes(2));

        var options = ReferentielMessagingOptions.Bind(configuration);
        if (!options.Enabled)
            return services;

        if (options.InitialSyncOnStartup)
            services.AddHostedService<ReferentielInitialSyncHostedService>();

        return services;
    }

    public static void AddReferentielConsumers(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<PaysReferentielConsumer>();
        configurator.AddConsumer<DeviseReferentielConsumer>();
        configurator.AddConsumer<EtatReferentielConsumer>();
        configurator.AddConsumer<PortReferentielConsumer>();
        configurator.AddConsumer<TauxDeChangeReferentielConsumer>();
        configurator.AddConsumer<ModeDeTransportsReferentielConsumer>();
        configurator.AddConsumer<AeroportReferentielConsumer>();
        configurator.AddConsumer<UniteStatistiqueReferentielConsumer>();
        configurator.AddConsumer<CorridorReferentielConsumer>();
        configurator.AddConsumer<DepartementReferentielConsumer>();
        configurator.AddConsumer<RouteNationaleReferentielConsumer>();
        configurator.AddConsumer<RouteReferentielConsumer>();
        configurator.AddConsumer<TronconReferentielConsumer>();
    }
}
