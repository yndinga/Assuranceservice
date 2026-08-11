using Microsoft.Extensions.Configuration;

namespace AssuranceService.Infrastructure.Messaging;

public class ReferentielMessagingOptions
{
    public const string SectionName = "ReferentielMessaging";

    public bool Enabled { get; set; }

    public bool InitialSyncOnStartup { get; set; } = true;

    public string ReferentielServiceBaseUrl { get; set; } = "http://localhost:62234";

    public RabbitMqHostOptions RabbitMq { get; set; } = new();

    public static ReferentielMessagingOptions Bind(IConfiguration configuration)
    {
        var options = configuration
            .GetSection(SectionName)
            .Get<ReferentielMessagingOptions>() ?? new ReferentielMessagingOptions();

        var rootMq = configuration.GetSection("RabbitMQ");
        if (!string.IsNullOrWhiteSpace(rootMq["Host"]))
            options.RabbitMq.Host = rootMq["Host"]!;
        if (!string.IsNullOrWhiteSpace(rootMq["User"]))
            options.RabbitMq.User = rootMq["User"]!;
        if (!string.IsNullOrWhiteSpace(rootMq["Password"]))
            options.RabbitMq.Password = rootMq["Password"]!;
        if (rootMq.GetValue<int?>("Port") is int port && port > 0)
            options.RabbitMq.Port = port;

        var messagingMq = configuration.GetSection($"{SectionName}:RabbitMq");
        if (!string.IsNullOrWhiteSpace(messagingMq["Host"]))
            options.RabbitMq.Host = messagingMq["Host"]!;
        if (!string.IsNullOrWhiteSpace(messagingMq["User"]))
            options.RabbitMq.User = messagingMq["User"]!;
        if (!string.IsNullOrWhiteSpace(messagingMq["Password"]))
            options.RabbitMq.Password = messagingMq["Password"]!;
        if (messagingMq.GetValue<int?>("Port") is int messagingPort && messagingPort > 0)
            options.RabbitMq.Port = messagingPort;

        var baseUrl = configuration["ReferentielService:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(baseUrl))
            options.ReferentielServiceBaseUrl = baseUrl;

        return options;
    }
}

public class RabbitMqHostOptions
{
    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string User { get; set; } = "guest";

    public string Password { get; set; } = "guest";
}
