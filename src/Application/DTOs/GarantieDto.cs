namespace AssuranceService.Application.DTOs;

/// <summary>
/// DTO pour Garantie
/// </summary>
public record GarantieDto
{
    public Guid ID { get; init; }
    
    public string NomGarantie { get; init; } = string.Empty;
    public decimal? Taux { get; init; }
    public decimal Accessoires { get; init; }
    public bool Actif { get; init; }
    
    // Audit
    public DateTime CreerLe { get; init; }
    public DateTime? ModifierLe { get; init; }
}
