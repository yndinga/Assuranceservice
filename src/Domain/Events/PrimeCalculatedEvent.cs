namespace AssuranceService.Domain.Events;

public record PrimeCalculatedEvent
{
    public Guid PrimeId { get; init; }
    public Guid AssuranceId { get; init; }
    public decimal ValeurFCFA { get; init; }
    public decimal ValeurDevise { get; init; }
    public decimal? PrimeNette { get; init; }
    public decimal? PrimeTotale { get; init; }
    public string Statut { get; init; } = string.Empty;
    public DateTime CalculatedAt { get; init; }
}





