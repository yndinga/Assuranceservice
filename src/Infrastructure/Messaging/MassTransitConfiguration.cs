using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AssuranceService.Application.Sagas;
using AssuranceService.Application.Consumers;
using AssuranceService.Domain.Events;
using AssuranceService.Infrastructure.Data;
using AssuranceService.Infrastructure.Messaging.Consumers;

namespace AssuranceService.Infrastructure.Messaging;

public static class MassTransitConfiguration
{
    public static IServiceCollection AddMassTransitWithRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMqSettings = configuration.GetSection("RabbitMQ");
        var connectionString = (rabbitMqSettings["ConnectionString"] ?? "").Trim();
        var enabled = rabbitMqSettings.GetValue<bool>("Enabled");
        var referentielOptions = ReferentielMessagingOptions.Bind(configuration);
        var useRabbitMq = (enabled && !string.IsNullOrEmpty(connectionString))
            || referentielOptions.Enabled;

        services.AddMassTransit(x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("AssuranceService", false));

            x.AddSagaStateMachine<AssuranceProcessStateMachine, AssuranceProcessState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ExistingDbContext<AssuranceDbContext>();
                    r.UseSqlServer();
                });

            x.AddConsumer<AssuranceCreatedConsumer>();
            x.AddConsumer<PrimeCalculatedConsumer>();
            x.AddConsumer<AssuranceProcessCompletedConsumer>();
            x.AddConsumer<AssuranceProcessFailedConsumer>();

            if (referentielOptions.Enabled)
                x.AddReferentielConsumers();

            if (useRabbitMq)
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    if (!string.IsNullOrEmpty(connectionString))
                        cfg.Host(connectionString);
                    else
                    {
                        var mq = referentielOptions.RabbitMq;
                        cfg.Host(mq.Host, (ushort)mq.Port, "/", h =>
                        {
                            h.Username(mq.User);
                            h.Password(mq.Password);
                        });
                    }

                    cfg.ConfigureEndpoints(context);
                    cfg.Message<AssuranceProcessStartedEvent>(e => e.SetEntityName("assurance.process.started"));
                    cfg.Message<AssuranceCreatedEvent>(e => e.SetEntityName("assurance.created"));
                    cfg.Message<PrimeCalculatedEvent>(e => e.SetEntityName("prime.calculated"));
                    cfg.Message<AssuranceProcessCompletedEvent>(e => e.SetEntityName("assurance.process.completed"));
                    cfg.Message<AssuranceProcessFailedEvent>(e => e.SetEntityName("assurance.process.failed"));
                    cfg.Message<AssuranceSubmittedEvent>(e => e.SetEntityName("assurance.submitted"));
                    cfg.Message<AssuranceSignedEvent>(e => e.SetEntityName("assurance.signed"));
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
