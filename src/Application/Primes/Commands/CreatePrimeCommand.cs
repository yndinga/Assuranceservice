using MediatR;

namespace AssuranceService.Application.Primes.Commands;

public record CreatePrimeCommand : IRequest<Guid>
{
    public string? Taux { get; init; }
    public decimal ValeurFCFA { get; init; }
    public decimal ValeurDevise { get; init; }
    public decimal? PrimeNette { get; init; }
    public double? Accessoires { get; init; }
    public decimal? Taxe { get; init; }
    public decimal? PrimeTotale { get; init; }
    public Guid AssuranceId { get; init; }
    public string Statut { get; init; } = string.Empty;
    // CreerPar et ModifierPar gérés automatiquement
}



