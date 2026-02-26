namespace AssuranceService.Domain.Events;

public record AssuranceUpdatedEvent
{
    public Guid AssuranceId { get; init; }
    public string NoPolice { get; init; } = string.Empty;
    public string Importateur { get; init; } = string.Empty;
    public string TypeContrat { get; init; } = string.Empty;
    public DateTime? DateDebut { get; init; }
    public DateTime? DateFin { get; init; }
    public string Statut { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
}





