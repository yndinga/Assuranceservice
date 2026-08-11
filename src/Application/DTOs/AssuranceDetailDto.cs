namespace AssuranceService.Application.DTOs;

/// <summary>
/// DTO détaillé pour Assurance avec les collections de navigation
/// </summary>
public record AssuranceDetailDto : AssuranceDto
{
    public ICollection<PrimeDto> Primes { get; init; } = new List<PrimeDto>();
    public ICollection<VisaAssuranceDto> Visas { get; init; } = new List<VisaAssuranceDto>();
    public ICollection<AssuranceDeclarationDto> Declarations { get; init; } = new List<AssuranceDeclarationDto>();
    public ICollection<AssuranceLigneDto> Lignes { get; init; } = new List<AssuranceLigneDto>();
}
