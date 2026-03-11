using AssuranceService.Application.Common;
using AssuranceService.Domain.Constants;
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
    private readonly IDocumentRepository _documentRepository;
    private readonly INumeroGeneratorService _numeroGeneratorService;
    private readonly IPrimeCalculatorService _primeCalculatorService;
    private readonly IPartenaireService _partenaireService;
    private readonly IPrimeRepository _primeRepository;

    public SubmitAssuranceHandler(
        IAssuranceRepository assuranceRepository,
        IMarchandiseRepository marchandiseRepository,
        IVoyageRepository voyageRepository,
        IDocumentRepository documentRepository,
        INumeroGeneratorService numeroGeneratorService,
        IPrimeCalculatorService primeCalculatorService,
        IPartenaireService partenaireService,
        IPrimeRepository primeRepository)
    {
        _assuranceRepository = assuranceRepository;
        _marchandiseRepository = marchandiseRepository;
        _voyageRepository = voyageRepository;
        _documentRepository = documentRepository;
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

        if (assurance.Statut != StatutAssuranceCodes.Elaboré)
        {
            throw new InvalidOperationException($"L'assurance doit être au statut Elaboré (10) pour être soumise. Statut actuel: {assurance.Statut}");
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

        // 5. Valider qu'il y a au moins un document
        var documents = await _documentRepository.GetByAssuranceIdAsync(request.AssuranceId);
        if (documents == null || !documents.Any())
        {
            throw new InvalidOperationException("L'assurance doit avoir au moins un document");
        }

        // 6. Assureur (garant) : doit déjà être renseigné (création directe ou action ChoisirAssureur par l'intermédiaire)
        if (!assurance.AssureurId.HasValue)
            throw new InvalidOperationException("Assureur requis pour la soumission. Renseignez l'assureur à la création ou utilisez l'action « Choisir l'assureur » si la demande a été envoyée à un intermédiaire.");
        var codePartenaire = await _partenaireService.GetCodePartenaireAsync(assurance.AssureurId.Value);

        // 7. Générer NoPolice et NumeroCert
        var noPolice = await _numeroGeneratorService.GenerateNoPoliceLAsync(codePartenaire);
        var numeroCert = await _numeroGeneratorService.GenerateNumeroCertAsync();

        var valeurTotaleMarchandises = marchandises.Sum(m => m.ValeurDevise ?? 0);
        var devise = marchandises.Select(m => m.Devise).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
        if (string.IsNullOrWhiteSpace(devise))
            throw new InvalidOperationException("Aucune marchandise ne possède de devise renseignée (Devise).");

        // 9. Calculer la prime — GarantieId vient de l'assurance (sauvegardé à la création)
        if (!assurance.GarantieId.HasValue)
            throw new InvalidOperationException("L'assurance ne possède pas de garantie associée (GarantieId requis).");

        var primeCalculation = await _primeCalculatorService.CalculerPrimeAsync(new PrimeCalculationRequest
        {
            AssuranceId = request.AssuranceId,
            GarantieId = assurance.GarantieId.Value,
            ValeurDevise = valeurTotaleMarchandises,
            Devise = devise
        });

        // 10. Créer l'objet Prime
        var prime = new Prime
        {
            AssuranceId = request.AssuranceId,
            Taux = primeCalculation.Taux,
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

        // 11. Via intermédiaire : créer la ligne VisaAssurance (signataire) — une seule ligne par assurance
        assurance.NoPolice = noPolice;
        assurance.NumeroCert = numeroCert;
        assurance.Statut = StatutAssuranceCodes.VisaDemandé;
        assurance.ModifierLe = DateTime.UtcNow;

        // VisaAssurance : écrit uniquement lors de la signature (pas ici)
        await _assuranceRepository.UpdateAsync(assurance);

        // 12. Retourner la réponse
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

