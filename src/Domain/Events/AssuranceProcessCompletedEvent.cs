namespace AssuranceService.Domain.Events;

public record AssuranceProcessCompletedEvent
{
    public Guid AssuranceId { get; init; }
    public string NoPolice { get; init; } = string.Empty;
    public string Statut { get; init; } = string.Empty;
    public DateTime CompletedAt { get; init; }
}





