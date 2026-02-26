namespace AssuranceService.Application.DTOs;

/// <summary>
/// DTO pour VisaAssurance
/// </summary>
public record VisaAssuranceDto
{
    public Guid ID { get; init; }
    public Guid AssuranceId { get; init; }
    public Guid OrganisationId { get; init; }
    public bool VisaOK { get; init; }
    public string? VisaContent { get; init; }
    
    // Audit
    public DateTime CreerLe { get; init; }
    public DateTime? ModifierLe { get; init; }
}
