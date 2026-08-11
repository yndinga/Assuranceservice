namespace AssuranceService.Application.DTOs;

/// <summary>
/// DTO pour VisaAssurance
/// </summary>
public record VisaAssuranceDto
{
    public Guid ID { get; init; }
    public Guid AssuranceId { get; init; }
    public string TypePartenaire { get; init; } = string.Empty;
    public string Organisation { get; init; } = string.Empty;
    public bool VisaOK { get; init; }
    public string? VisaContent { get; init; }
    public string? Message { get; init; }
    public string? Licence { get; init; }
    public DateTime? DateVisa { get; init; }
    public string Statut { get; init; } = string.Empty;
    
    // Audit
    public DateTime CreerLe { get; init; }
    public DateTime? ModifierLe { get; init; }
}
