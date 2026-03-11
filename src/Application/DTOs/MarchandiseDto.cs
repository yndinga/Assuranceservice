namespace AssuranceService.Application.DTOs;

/// <summary>
/// DTO pour Marchandise
/// </summary>
public record MarchandiseDto
{
    public Guid ID { get; init; }
    public Guid AssuranceId { get; init; }
    
    public string Designation { get; init; } = string.Empty;
    public string? Nature { get; init; }
    public string? Specificites { get; init; }
    public string Conditionnement { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal ValeurFCFA { get; init; }
    public decimal? ValeurDevise { get; init; }
    public string Devise { get; init; } = string.Empty;
    public string? MasseBrute { get; init; }
    public string? UniteStatistique { get; init; }
    public string? Marque { get; init; }
    
    // Audit
    public DateTime CreerLe { get; init; }
    public DateTime? ModifierLe { get; init; }
}
