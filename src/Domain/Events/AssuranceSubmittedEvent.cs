namespace AssuranceService.Domain.Events;

public record AssuranceSubmittedEvent
{
    public Guid AssuranceId { get; init; }
    public string NoPolice { get; init; } = string.Empty;
    public string NumeroCert { get; init; } = string.Empty;
    public string? NoFacture { get; init; }
    public string? ImportateurNom { get; init; }
    public string? ImportateurNIU { get; init; }
    public string? Partenaire { get; init; }
    public string? TypePartenaire { get; init; }
    public string? Signataire { get; init; }
    public string Statut { get; init; } = string.Empty;
    public DateTime SubmittedAt { get; init; }
}
