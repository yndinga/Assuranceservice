namespace AssuranceService.Application.Common;

/// <summary>
/// Service pour calculer la prime d'assurance
/// </summary>
public interface IPrimeCalculatorService
{
    /// <summary>
    /// Calcule la prime totale d'une assurance
    /// </summary>
    Task<PrimeCalculationResult> CalculerPrimeAsync(PrimeCalculationRequest request);
}

public record PrimeCalculationRequest
{
    public Guid AssuranceId { get; init; }
    public Guid GarantieId { get; init; }
    public decimal ValeurDevise { get; init; }
    public string Devise { get; init; } = string.Empty;
}

public record PrimeCalculationResult
{
    public decimal ValeurFCFA { get; init; }
    public decimal Taux { get; init; }
    public decimal Accessoires { get; init; }
    public decimal PrimeNette { get; init; }
    public decimal Taxe { get; init; }
    public decimal PrimeTotale { get; init; }
}



