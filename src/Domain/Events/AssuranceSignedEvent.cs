namespace AssuranceService.Domain.Events;

public record AssuranceSignedEvent
{
    public Guid AssuranceId { get; init; }
    public string? NoPolice { get; init; }
    public string? NumeroCert { get; init; }
    public string? NoFacture { get; init; }
    public string? ImportateurNom { get; init; }
    public string? ImportateurNIU { get; init; }
    public string? TypePartenaire { get; init; }
    public string Signataire { get; init; } = string.Empty;
    public string Decision { get; init; } = string.Empty;
    public bool VisaOK { get; init; }
    public DateTime? DateDebut { get; init; }
    public DateTime? DateFin { get; init; }
    public DateTime SignedAt { get; init; }
}
