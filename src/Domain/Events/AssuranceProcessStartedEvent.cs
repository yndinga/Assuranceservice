namespace AssuranceService.Domain.Events;

public record AssuranceProcessStartedEvent
{
    public Guid AssuranceId { get; init; }
    public string NoPolice { get; init; } = string.Empty;
    public string TypeContrat { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
}





