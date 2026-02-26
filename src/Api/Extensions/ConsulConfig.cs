namespace AssuranceService.Api.Extensions;

public class ConsulConfig
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string ServiceAddress { get; set; } = string.Empty;
    public int ServicePort { get; set; }
    public string HealthCheckPath { get; set; } = "/health";
    public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan DeregisterAfter { get; set; } = TimeSpan.FromMinutes(1);
}




