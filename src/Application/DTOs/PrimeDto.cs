namespace AssuranceService.Application.DTOs;

/// <summary>
/// DTO pour Prime
/// </summary>
public record PrimeDto
{
    public Guid ID { get; init; }
    public Guid AssuranceId { get; init; }
    
    // Taux et valeurs
    public string? Taux { get; init; }
    public decimal ValeurFCFA { get; init; }
    public decimal ValeurDevise { get; init; }
    
    // Calculs
    public decimal? PrimeNette { get; init; }
    public double? Accessoires { get; init; }
    public decimal? Taxe { get; init; }
    public decimal? PrimeTotale { get; init; }
        // Statut
    public string Statut { get; init; } = string.Empty;
    
    // Audit
    public DateTime CreerLe { get; init; }
    public DateTime? ModifierLe { get; init; }
}
