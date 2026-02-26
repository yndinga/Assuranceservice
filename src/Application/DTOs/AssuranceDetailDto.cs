namespace AssuranceService.Application.DTOs;

/// <summary>
/// DTO détaillé pour Assurance avec les collections de navigation
/// </summary>
public record AssuranceDetailDto : AssuranceDto
{
    public ICollection<MarchandiseDto> Marchandises { get; init; } = new List<MarchandiseDto>();
    public ICollection<PrimeDto> Primes { get; init; } = new List<PrimeDto>();
    public ICollection<VoyageDto> Voyages { get; init; } = new List<VoyageDto>();
    public ICollection<VisaAssuranceDto> Visas { get; init; } = new List<VisaAssuranceDto>();
}
