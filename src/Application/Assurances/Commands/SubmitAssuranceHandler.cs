using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

/// <summary>
/// Handler pour soumettre une assurance et générer les numéros + calculer la prime
/// </summary>
public class SubmitAssuranceHandler : IRequestHandler<SubmitAssuranceCommand, SubmitAssuranceResponse>
{
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly IMarchandiseRepository _marchandiseRepository;
    private readonly IVoyageRepository _voyageRepository;
    private readonly INumeroGeneratorService _numeroGeneratorService;
    private readonly IPrimeCalculatorService _primeCalculatorService;
    private readonly IPartenaireService _partenaireService;
    private readonly IPrimeRepository _primeRepository;

    public SubmitAssuranceHandler(
        IAssuranceRepository assuranceRepository,
        IMarchandiseRepository marchandiseRepository,
        IVoyageRepository voyageRepository,
        INumeroGeneratorService numeroGeneratorService,
        IPrimeCalculatorService primeCalculatorService,
        IPartenaireService partenaireService,
        IPrimeRepository primeRepository)
    {
        _assuranceRepository = assuranceRepository;
        _marchandiseRepository = marchandiseRepository;
        _voyageRepository = voyageRepository;
        _numeroGeneratorService = numeroGeneratorService;
        _primeCalculatorService = primeCalculatorService;
        _partenaireService = partenaireService;
        _primeRepository = primeRepository;
    }

    public async Task<SubmitAssuranceResponse> Handle(SubmitAssuranceCommand request, CancellationToken cancellationToken)
    {
        // 1. Valider que l'assurance existe
        var assurance = await _assuranceRepository.GetByIdAsync(request.AssuranceId);
        if (assurance == null)
        {
            throw new InvalidOperationException($"Assurance {request.AssuranceId} introuvable");
        }

        // 2. Valider le statut
        if (assurance.Statut != "Elaborer")
        {
            throw new InvalidOperationException($"L'assurance doit être au statut 'Elaborer' pour être soumise. Statut actuel: {assurance.Statut}");
        }

        // 3. Valider qu'il y a au moins une marchandise
        var marchandises = await _marchandiseRepository.GetByAssuranceIdAsync(request.AssuranceId);
        if (marchandises == null || !marchandises.Any())
        {
            throw new InvalidOperationException("L'assurance doit avoir au moins une marchandise");
        }

        // 4. Valider qu'il y a un voyage
        var voyages = await _voyageRepository.GetByAssuranceIdAsync(request.AssuranceId);
        if (voyages == null || !voyages.Any())
        {
            throw new InvalidOperationException("L'assurance doit avoir un voyage");
        }

        // 5. Assureur (garant) : AssureurId (création directe) ou request.PartenaireId (courtier a sélectionné)
        var partenaireId = request.PartenaireId ?? assurance.AssureurId
            ?? throw new InvalidOperationException("Assureur requis pour la soumission.");
        var codePartenaire = await _partenaireService.GetCodePartenaireAsync(partenaireId);

        // 6. Générer NoPolice et NumeroCert
        var noPolice = await _numeroGeneratorService.GenerateNoPoliceLAsync(codePartenaire);
        var numeroCert = await _numeroGeneratorService.GenerateNumeroCertAsync();

        // 7. Calculer la valeur totale des marchandises
        //    La Devise vient de la marchandise (Marchandise.Devise), pas de la commande
        var valeurTotaleMarchandises = marchandises.Sum(m => m.ValeurDevise ?? 0);
        var devise = marchandises
            .Where(m => !string.IsNullOrWhiteSpace(m.Devise))
            .Select(m => m.Devise)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Aucune marchandise ne possède de devise renseignée.");

        // 8. Calculer la prime — GarantieId vient de l'assurance (sauvegardé à la création)
        if (!assurance.GarantieId.HasValue)
            throw new InvalidOperationException("L'assurance ne possède pas de garantie associée (GarantieId requis).");

        var primeCalculation = await _primeCalculatorService.CalculerPrimeAsync(new PrimeCalculationRequest
        {
            AssuranceId = request.AssuranceId,
            GarantieId = assurance.GarantieId.Value,
            ValeurDevise = valeurTotaleMarchandises,
            Devise = devise
        });

        // 9. Créer l'objet Prime
        var prime = new Prime
        {
            AssuranceId = request.AssuranceId,
            Taux = primeCalculation.Taux.ToString(),
            ValeurFCFA = primeCalculation.ValeurFCFA,
            ValeurDevise = valeurTotaleMarchandises,
            PrimeNette = primeCalculation.PrimeNette,
            Accessoires = (double)primeCalculation.Accessoires,
            Taxe = primeCalculation.Taxe,
            PrimeTotale = primeCalculation.PrimeTotale,
            Statut = "Calculée",
            CreerPar = "System",
            ModifierPar = "System",
            CreerLe = DateTime.UtcNow
        };

        await _primeRepository.CreateAsync(prime);

        // 10. Via intermédiaire : créer la ligne VisaAssurance (signataire) — une seule ligne par assurance
        assurance.NoPolice = noPolice;
        assurance.NumeroCert = numeroCert;
        assurance.Statut = "VisaDemandé";
        assurance.ModifierLe = DateTime.UtcNow;

        if (request.PartenaireId.HasValue)
            assurance.AssureurId = request.PartenaireId.Value;

        // VisaAssurance : écrit uniquement lors de la signature (pas ici)
        await _assuranceRepository.UpdateAsync(assurance);

        // 11. Retourner la réponse
        return new SubmitAssuranceResponse
        {
            AssuranceId = assurance.Id,
            NoPolice = noPolice,
            NumeroCert = numeroCert,
            ValeurFCFA = primeCalculation.ValeurFCFA,
            PrimeNette = primeCalculation.PrimeNette,
            Accessoires = primeCalculation.Accessoires,
            Taxe = primeCalculation.Taxe,
            PrimeTotale = primeCalculation.PrimeTotale,
            Statut = assurance.Statut
        };
    }
}

