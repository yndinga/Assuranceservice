using AssuranceService.Application.Common;
using AssuranceService.Domain.Constants;
using AssuranceService.Domain.Events;
using AssuranceService.Domain.Models;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AssuranceService.Application.Assurances.Commands;

/// <summary>
/// Handler pour soumettre une assurance et gÃ©nÃ©rer les numÃ©ros + calculer la prime
/// </summary>
public class SubmitAssuranceHandler : IRequestHandler<SubmitAssuranceCommand, SubmitAssuranceResponse>
{
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IPrimeRepository _primeRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SubmitAssuranceHandler> _logger;
    private readonly IDeclarationDossierClient _declarationClient;

    public SubmitAssuranceHandler(
        IAssuranceRepository assuranceRepository,
        IDocumentRepository documentRepository,
        IPrimeRepository primeRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<SubmitAssuranceHandler> logger,
        IDeclarationDossierClient declarationClient)
    {
        _assuranceRepository = assuranceRepository;
        _documentRepository = documentRepository;
        _primeRepository = primeRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _declarationClient = declarationClient;
    }

    public async Task<SubmitAssuranceResponse> Handle(SubmitAssuranceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Soumission assurance demandee. AssuranceId={AssuranceId}", request.AssuranceId);

        // 1. Valider que l'assurance existe
        var assurance = await _assuranceRepository.GetByIdAsync(request.AssuranceId);
        if (assurance == null)
        {
            throw new InvalidOperationException($"Assurance {request.AssuranceId} introuvable");
        }

        if (StatutAssuranceCodes.IsRefuse(assurance.Etat))
        {
            throw new InvalidOperationException("Une assurance refusee est terminale et ne peut pas etre soumise a nouveau.");
        }

        if (!StatutAssuranceCodes.IsElabore(assurance.Etat) && !StatutAssuranceCodes.IsModificationDemandee(assurance.Etat))
        {
            throw new InvalidOperationException($"L'assurance doit etre au statut Elabore (42) ou Modification demandee (66) pour etre soumise. Statut actuel: {assurance.Etat}");
        }

        await ValidateDeclarationSourcesAsync(assurance, cancellationToken);

        // 3. Valider qu'il y a au moins un document (existence par AssuranceId uniquement)
        var hasDocument = await _documentRepository.ExistsByAssuranceIdAsync(request.AssuranceId, cancellationToken);
        if (!hasDocument)
        {
            throw new InvalidOperationException("L'assurance doit contenir au moins une facture.");
        }

        // 4. Valider qu'il y a une prime dÃ©jÃ  calculÃ©e
        var prime = (await _primeRepository.GetByAssuranceIdAsync(request.AssuranceId))
            .OrderByDescending(p => p.CreerLe)
            .FirstOrDefault();
        if (prime == null)
        {
            throw new InvalidOperationException("L'assurance doit avoir une prime");
        }

        // 5. Assureur porteur/facturant : requis avant soumission.
        var codePartenaire = NormalizeRequired(assurance.Partenaire, "Assureur requis pour la soumission.");
        var typePartenaire = NormalizeTypePartenaire(assurance.TypePartenaire);
        var codeSignataire = typePartenaire == TypePartenaireCodes.Assureur
            ? codePartenaire
            : NormalizeRequired(assurance.Intermediaire, "Intermediaire signataire requis pour la soumission.");

        if (typePartenaire == TypePartenaireCodes.Assureur)
        {
            assurance.Intermediaire = null;
        }

        // 6. La numerotation est reservee a la signature de l'assurance.

        // 7. Via intermÃ©diaire : crÃ©er la ligne VisaAssurance (signataire) â€” une seule ligne par assurance
        assurance.Etat = StatutAssuranceCodes.IsModificationDemandee(assurance.Etat)
            ? StatutAssuranceCodes.ModificationSoumise
            : StatutAssuranceCodes.VisaDemande;
        assurance.ModifierLe = DateTime.UtcNow;

        // VisaAssurance : Ã©crit uniquement lors de la signature (pas ici)
        var visa = assurance.Visas.FirstOrDefault(item =>
            string.Equals(item.TypePartenaire, typePartenaire, StringComparison.OrdinalIgnoreCase)
            && string.Equals(item.Organisation, codeSignataire, StringComparison.OrdinalIgnoreCase));
        if (visa is null)
        {
            visa = new VisaAssurance
            {
                Id = Guid.NewGuid(),
                AssuranceId = assurance.Id,
                CreerPar = "System",
                CreerLe = DateTime.UtcNow
            };
            await _assuranceRepository.AddVisaAssuranceAsync(visa);
        }

        visa.TypePartenaire = typePartenaire;
        visa.Organisation = codeSignataire;
        visa.VisaOK = false;
        visa.VisaContent = null;
        visa.Message = null;
        visa.Licence = null;
        visa.DateVisa = null;
        visa.Statut = "EN_ATTENTE";
        visa.ModifierPar = "System";
        visa.ModifierLe = DateTime.UtcNow;

        await _assuranceRepository.UpdateAsync(assurance);

        var submittedAt = assurance.ModifierLe ?? DateTime.UtcNow;
        await _publishEndpoint.Publish(new AssuranceSubmittedEvent
        {
            AssuranceId = assurance.Id,
            NoPolice = assurance.NoPolice ?? string.Empty,
            NumeroCert = assurance.NumeroCert ?? string.Empty,
            NoFacture = assurance.NoFacture,
            ImportateurNom = assurance.ImportateurNom,
            ImportateurNIU = assurance.ImportateurNIU,
            Partenaire = codePartenaire,
            TypePartenaire = typePartenaire,
            Signataire = codeSignataire,
            Statut = assurance.Etat,
            SubmittedAt = submittedAt
        }, cancellationToken);

        _logger.LogInformation(
            "Assurance soumise. AssuranceId={AssuranceId} NoPolice={NoPolice} NumeroCert={NumeroCert} NoFacture={NoFacture} Partenaire={Partenaire} Signataire={Signataire} TypePartenaire={TypePartenaire} Statut={Statut}",
            assurance.Id,
            assurance.NoPolice,
            assurance.NumeroCert,
            assurance.NoFacture,
            codePartenaire,
            codeSignataire,
            typePartenaire,
            assurance.Etat);

        // 8. Retourner la rÃ©ponse
        return new SubmitAssuranceResponse
        {
            AssuranceId = assurance.Id,
            NoPolice = assurance.NoPolice ?? string.Empty,
            NumeroCert = assurance.NumeroCert ?? string.Empty,
            ValeurFCFA = prime.ValeurFCFA,
            PrimeNette = prime.PrimeNette ?? 0m,
            Accessoires = (decimal)(prime.Accessoires ?? 0d),
            Taxe = prime.Taxe ?? 0m,
            PrimeTotale = prime.PrimeTotale ?? 0m,
            Statut = assurance.Etat
        };
    }

    private static string NormalizeTypePartenaire(string? value)
    {
        var normalized = NormalizeOptional(value)?.ToUpperInvariant();
        return normalized is TypePartenaireCodes.Courtier or TypePartenaireCodes.AgentGeneral
            ? normalized
            : TypePartenaireCodes.Assureur;
    }

    private static string NormalizeRequired(string? value, string message)
    {
        var normalized = NormalizeOptional(value);
        if (normalized is null)
            throw new InvalidOperationException(message);

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private async Task ValidateDeclarationSourcesAsync(Assurance assurance, CancellationToken cancellationToken)
    {
        if (assurance.Declarations.Count == 0 && assurance.Lignes.Count == 0)
            return; // Compatibilité des assurances historiques créées avant le modèle Assurance-DI.

        if (assurance.Declarations.Count == 0 || assurance.Lignes.Count == 0)
            throw new InvalidOperationException("Les DI et les lignes sources de l'assurance sont incomplètes.");

        var dossierTasks = assurance.Declarations
            .Select(item => _declarationClient.GetDossierAsync(item.DeclarationId, cancellationToken));
        var dossiers = await Task.WhenAll(dossierTasks);
        if (dossiers.Any(dossier => dossier is null))
            throw new InvalidOperationException("Une DI source est introuvable ou n'est plus visible.");

        var dossiersById = dossiers.Select(dossier => dossier!)
            .ToDictionary(dossier => dossier.Id);
        foreach (var dossier in dossiersById.Values)
        {
            if (dossier.Etat != 50)
                throw new InvalidOperationException($"La DI {dossier.NoDossier ?? dossier.Id.ToString()} n'est plus ouverte (état {dossier.Etat}).");
        }

        var consumptions = await _assuranceRepository.GetLineConsumptionsAsync(
            assurance.Lignes.Select(line => line.SourceLigneDIId).Distinct().ToArray(),
            assurance.Id,
            cancellationToken);

        string? commonCountry = null;
        string? commonPort = null;
        foreach (var declaration in assurance.Declarations)
        {
            var dossier = dossiersById[declaration.DeclarationId];
            var declarationLines = assurance.Lignes.Where(line => line.DeclarationId == declaration.DeclarationId).ToArray();
            if (declarationLines.Length == 0)
                throw new InvalidOperationException($"La DI {declaration.NumeroDI ?? declaration.DeclarationId.ToString()} ne contient aucune ligne assurée.");

            foreach (var insuredLine in declarationLines)
            {
                var source = dossier.Commandes
                    .SelectMany(commande => commande.LignesCommandes.Select(line => new { Commande = commande, Ligne = line }))
                    .SingleOrDefault(item => item.Ligne.Id == insuredLine.SourceLigneDIId);
                if (source is null)
                    throw new InvalidOperationException($"La ligne DI source {insuredLine.SourceLigneDIId} est introuvable.");

                var country = NormalizeCode(source.Commande.PaysEmbarquement ?? source.Commande.PaysProvenance)
                    ?? throw new InvalidOperationException("Le pays d'embarquement est absent d'une DI source.");
                var port = NormalizeCode(source.Commande.PortEmbarquement
                                         ?? source.Commande.FleuveEmbarquement
                                         ?? source.Commande.AeroportEmbarquement
                                         ?? source.Commande.RouteNationale)
                    ?? throw new InvalidOperationException("Le port ou point d'embarquement est absent d'une DI source.");

                commonCountry ??= country;
                commonPort ??= port;
                if (!string.Equals(country, commonCountry, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(port, commonPort, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Les DI sources n'ont plus le même pays et le même port d'embarquement.");

                if (!string.Equals(country, declaration.PaysEmbarquementCode, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(port, declaration.PortEmbarquementCode, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Les données d'embarquement de la DI {declaration.NumeroDI ?? declaration.DeclarationId.ToString()} ont changé.");

                var consumed = consumptions.GetValueOrDefault(insuredLine.SourceLigneDIId)
                    ?? new AssuranceLineConsumption(0m, 0m, 0m, 0m, 0m, 0m);
                EnsureWithinSource("quantité", insuredLine.Quantite, consumed.Quantite, source.Ligne.Quantite, insuredLine.SourceLigneDIId);
                EnsureWithinSource("masse brute", insuredLine.MasseBrute, consumed.MasseBrute, source.Ligne.MasseBrute, insuredLine.SourceLigneDIId);
                EnsureWithinSource("masse nette", insuredLine.MasseNette, consumed.MasseNette, source.Ligne.MasseNette, insuredLine.SourceLigneDIId);
                EnsureWithinSource("volume", insuredLine.Volume, consumed.Volume, source.Ligne.Volume, insuredLine.SourceLigneDIId);
                EnsureWithinSource("valeur en devise", insuredLine.ValeurDevise, consumed.ValeurDevise, source.Ligne.ValeurDevise, insuredLine.SourceLigneDIId);
                EnsureWithinSource("valeur XAF", insuredLine.ValeurXAF, consumed.ValeurXAF, source.Ligne.ValeurFCFA, insuredLine.SourceLigneDIId);
            }
        }
    }

    private static void EnsureWithinSource(
        string label,
        decimal? insured,
        decimal consumedByOtherAssurances,
        decimal? source,
        Guid sourceLineId)
    {
        var insuredValue = insured ?? 0m;
        var sourceValue = source ?? 0m;
        if (insuredValue < 0m || insuredValue + consumedByOtherAssurances > sourceValue)
            throw new InvalidOperationException(
                $"La {label} de la ligne DI {sourceLineId} dépasse le reliquat disponible au moment de la soumission.");
    }

    private static string? NormalizeCode(string? value) => NormalizeOptional(value)?.ToUpperInvariant();
}
