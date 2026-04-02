using AssuranceService.Application.Common;

namespace AssuranceService.Application.Services;

/// <summary>
/// Service de calcul de la prime d'assurance
/// </summary>
public class PrimeCalculatorService : IPrimeCalculatorService
{
    private readonly ITauxChangeService _tauxChangeService;
    private readonly IGarantieRepository _garantieRepository;

    public PrimeCalculatorService(
        ITauxChangeService tauxChangeService,
        IGarantieRepository garantieRepository)
    {
        _tauxChangeService = tauxChangeService;
        _garantieRepository = garantieRepository;
    }

    /// <summary>
    /// Calcule la prime selon la formule métier:
    /// 1. ValeurFCFA = ValeurDevise × TauxChange
    /// 2. Prime = ValeurFCFA × Taux
    /// 3. Prime minimale selon taux (10000 ou 17500)
    /// 4. Taxe = (Accessoires + Prime) × 15%
    /// 5. Total = Prime + Taxe + Accessoires
    /// </summary>
    public async Task<PrimeCalculationResult> CalculerPrimeAsync(PrimeCalculationRequest request)
    {
        // 1. Obtenir le taux de change
        var tauxChange = await _tauxChangeService.GetTauxChangeAsync(request.Devise);

        // 2. Convertir en FCFA
        var valeurFCFA = request.ValeurDevise * tauxChange;

        // 3. Obtenir les infos de garantie
        var garantie = await _garantieRepository.GetByIdAsync(request.GarantieId);
        if (garantie == null)
        {
            throw new InvalidOperationException($"Garantie {request.GarantieId} introuvable");
        }

        if (!garantie.Taux.HasValue || garantie.Taux.Value < 0)
        {
            throw new InvalidOperationException($"Taux de garantie invalide: {garantie.Taux}");
        }
        var tauxGarantie = garantie.Taux.Value;

        var accessoires = garantie.Accessoires;

        // 4. Calculer la prime nette
        var primeNette = valeurFCFA * tauxGarantie;

        // 5. Appliquer les minimums selon le taux
        primeNette = AppliquerPrimeMinimale(primeNette, tauxGarantie);

        // 6. Calculer la taxe (15%)
        var taxe = (accessoires + primeNette) * 0.15m;

        // 7. Calculer le total
        var primeTotale = primeNette + taxe + accessoires;

        return new PrimeCalculationResult
        {
            ValeurFCFA = valeurFCFA,
            Taux = tauxGarantie,
            Accessoires = accessoires,
            PrimeNette = primeNette,
            Taxe = taxe,
            PrimeTotale = primeTotale
        };
    }

    private decimal AppliquerPrimeMinimale(decimal prime, decimal taux)
    {
        // Si Taux = 0.002 et Prime < 10000 → Prime = 10000
        if (taux == 0.002m && prime < 10000)
        {
            return 10000;
        }

        // Si Taux = 0.0035 et Prime < 17500 → Prime = 17500
        if (taux == 0.0035m && prime < 17500)
        {
            return 17500;
        }

        return prime;
    }
}

