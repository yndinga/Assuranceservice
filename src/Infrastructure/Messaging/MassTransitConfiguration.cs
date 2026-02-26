using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AssuranceService.Application.Sagas;
using AssuranceService.Application.Consumers;
using AssuranceService.Domain.Events;
using AssuranceService.Infrastructure.Data;

namespace AssuranceService.Infrastructure.Messaging;

public static class MassTransitConfiguration
{
    /// <summary>
    /// Enregistre MassTransit : RabbitMQ si configuré (RabbitMQ__ConnectionString non vide et RabbitMQ__Enabled != false),
    /// sinon transport In-Memory pour que l'app démarre sans broker externe.
    /// </summary>
    public static IServiceCollection AddMassTransitWithRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMqSettings = configuration.GetSection("RabbitMQ");
        var connectionString = (rabbitMqSettings["ConnectionString"] ?? "").Trim();
        var enabled = rabbitMqSettings.GetValue<bool>("Enabled");
        // RabbitMQ uniquement si explicitement activé ET chaîne de connexion fournie (sinon In-Memory)
        var useRabbitMq = enabled && !string.IsNullOrEmpty(connectionString);

        services.AddMassTransit(x =>
        {
            x.AddSagaStateMachine<AssuranceProcessStateMachine, AssuranceProcessState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ExistingDbContext<AssuranceDbContext>();
                    r.UseSqlServer();
                });

            x.AddConsumer<AssuranceCreatedConsumer>();
            x.AddConsumer<MarchandiseAddedConsumer>();
            x.AddConsumer<PrimeCalculatedConsumer>();
            x.AddConsumer<AssuranceProcessCompletedConsumer>();
            x.AddConsumer<AssuranceProcessFailedConsumer>();

            if (useRabbitMq)
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(connectionString);
                    cfg.ConfigureEndpoints(context);
                    cfg.Message<AssuranceProcessStartedEvent>(e => e.SetEntityName("assurance.process.started"));
                    cfg.Message<AssuranceCreatedEvent>(e => e.SetEntityName("assurance.created"));
                    cfg.Message<MarchandiseAddedEvent>(e => e.SetEntityName("marchandise.added"));
                    cfg.Message<PrimeCalculatedEvent>(e => e.SetEntityName("prime.calculated"));
                    cfg.Message<AssuranceProcessCompletedEvent>(e => e.SetEntityName("assurance.process.completed"));
                    cfg.Message<AssuranceProcessFailedEvent>(e => e.SetEntityName("assurance.process.failed"));
                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    cfg.UseInMemoryOutbox(context);
                });
            }
            else
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                });
            }
        });

        return services;
    }
}

