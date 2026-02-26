namespace AssuranceService.Domain.Events;

public record MarchandiseAddedEvent
{
    public Guid MarchandiseId { get; init; }
    public Guid AssuranceId { get; init; }
    public string Designation { get; init; } = string.Empty;
    public decimal Valeur { get; init; }
    public string Conditionnement { get; init; } = string.Empty;
    public DateTime AddedAt { get; init; }
}





