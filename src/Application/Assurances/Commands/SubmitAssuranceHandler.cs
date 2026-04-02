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
    private readonly IDocumentRepository _documentRepository;
    private readonly INumeroGeneratorService _numeroGeneratorService;
    private readonly IPartenaireService _partenaireService;
    private readonly IPrimeRepository _primeRepository;

    public SubmitAssuranceHandler(
        IAssuranceRepository assuranceRepository,
        IDocumentRepository documentRepository,
        INumeroGeneratorService numeroGeneratorService,
        IPartenaireService partenaireService,
        IPrimeRepository primeRepository)
    {
        _assuranceRepository = assuranceRepository;
        _documentRepository = documentRepository;
        _numeroGeneratorService = numeroGeneratorService;
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

        // 3. Valider qu'il y a au moins un document (existence par AssuranceId uniquement)
        var hasDocument = await _documentRepository.ExistsByAssuranceIdAsync(request.AssuranceId, cancellationToken);
        if (!hasDocument)
        {
            throw new InvalidOperationException("L'assurance doit contenir au moins une facture.");
        }

        // 4. Valider qu'il y a une prime déjà calculée
        var prime = (await _primeRepository.GetByAssuranceIdAsync(request.AssuranceId))
            .OrderByDescending(p => p.CreerLe)
            .FirstOrDefault();
        if (prime == null)
        {
            throw new InvalidOperationException("L'assurance doit avoir une prime");
        }

        // 5. Assureur (garant) : doit déjà être renseigné (création directe ou action ChoisirAssureur par l'intermédiaire)
        if (!assurance.AssureurId.HasValue)
            throw new InvalidOperationException("Assureur requis pour la soumission. Renseignez l'assureur à la création ou utilisez l'action « Choisir l'assureur » si la demande a été envoyée à un intermédiaire.");
        var codePartenaire = await _partenaireService.GetCodePartenaireAsync(assurance.AssureurId.Value);

        // 6. Générer NoPolice et NumeroCert
        var noPolice = await _numeroGeneratorService.GenerateNoPoliceLAsync(codePartenaire);
        var numeroCert = await _numeroGeneratorService.GenerateNumeroCertAsync();

        // 7. Via intermédiaire : créer la ligne VisaAssurance (signataire) — une seule ligne par assurance
        assurance.NoPolice = noPolice;
        assurance.NumeroCert = numeroCert;
        assurance.Statut = StatutAssuranceCodes.VisaDemandé;
        assurance.ModifierLe = DateTime.UtcNow;

        // VisaAssurance : écrit uniquement lors de la signature (pas ici)
        await _assuranceRepository.UpdateAsync(assurance);

        // 8. Retourner la réponse
        return new SubmitAssuranceResponse
        {
            AssuranceId = assurance.Id,
            NoPolice = noPolice,
            NumeroCert = numeroCert,
            ValeurFCFA = prime.ValeurFCFA,
            PrimeNette = prime.PrimeNette ?? 0m,
            Accessoires = (decimal)(prime.Accessoires ?? 0d),
            Taxe = prime.Taxe ?? 0m,
            PrimeTotale = prime.PrimeTotale ?? 0m,
            Statut = assurance.Statut
        };
    }
}

