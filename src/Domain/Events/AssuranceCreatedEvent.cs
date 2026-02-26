namespace AssuranceService.Domain.Events;

public record AssuranceCreatedEvent
{
    public Guid AssuranceId { get; init; }
    public string NoPolice { get; init; } = string.Empty;
    public string Importateur { get; init; } = string.Empty;
    public string TypeContrat { get; init; } = string.Empty;
    public DateTime? DateDebut { get; init; }
    public DateTime? DateFin { get; init; }
    public string NomTransporteur { get; init; } = string.Empty;
    public string NomNavire { get; init; } = string.Empty;
    public string TypeNavire { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}





