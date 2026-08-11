using AssuranceService.Application.Common;
using AssuranceService.Domain.Constants;
using AssuranceService.Domain.Events;
using AssuranceService.Domain.Models;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AssuranceService.Application.Assurances.Commands;

public class SignerAssuranceHandler : IRequestHandler<SignerAssuranceCommand, Unit>
{
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly INumeroGeneratorService _numeroGeneratorService;
    private readonly IPartenaireService _partenaireService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SignerAssuranceHandler> _logger;

    public SignerAssuranceHandler(
        IAssuranceRepository assuranceRepository,
        INumeroGeneratorService numeroGeneratorService,
        IPartenaireService partenaireService,
        IPublishEndpoint publishEndpoint,
        ILogger<SignerAssuranceHandler> logger)
    {
        _assuranceRepository = assuranceRepository;
        _numeroGeneratorService = numeroGeneratorService;
        _partenaireService = partenaireService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Unit> Handle(SignerAssuranceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Signature assurance demandee. AssuranceId={AssuranceId} Organisation={Organisation} TypePartenaire={TypePartenaire} Decision={Decision}",
            request.AssuranceId,
            request.Organisation,
            request.TypePartenaire,
            request.Decision);

        var assurance = await _assuranceRepository.GetByIdAsync(request.AssuranceId);
        if (assurance == null)
        {
            throw new InvalidOperationException($"Assurance {request.AssuranceId} introuvable.");
        }

        if (!StatutAssuranceCodes.IsVisaDemande(assurance.Etat) && !StatutAssuranceCodes.IsModificationSoumise(assurance.Etat))
        {
            throw new InvalidOperationException(
                $"La signature n'est possible que pour une assurance en Visa demande (79) ou Modification soumise (68). Statut actuel : {assurance.Etat}.");
        }

        var requestedType = request.TypePartenaire.Trim().ToUpperInvariant();
        var requestedOrganisation = request.Organisation.Trim();
        var visa = assurance.Visas.FirstOrDefault(item =>
            string.Equals(item.TypePartenaire, requestedType, StringComparison.OrdinalIgnoreCase)
            && string.Equals(item.Organisation, requestedOrganisation, StringComparison.OrdinalIgnoreCase));
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

        var decision = StatutAssuranceCodes.NormalizeDecision(request.Decision);

        visa.TypePartenaire = requestedType;
        visa.Organisation = requestedOrganisation;
        visa.VisaOK = decision == StatutAssuranceCodes.Valide;
        visa.VisaContent = request.VisaContent;
        visa.Statut = decision;
        visa.DateVisa = DateTime.UtcNow;
        visa.ModifierPar = "System";
        visa.ModifierLe = DateTime.UtcNow;

        assurance.Etat = decision;
        if (decision == StatutAssuranceCodes.Valide)
        {
            var signedAt = DateTime.UtcNow;
            var durationDays = assurance.DureeJours ?? ParseDurationDays(assurance.Duree);

            if (string.IsNullOrWhiteSpace(assurance.NoPolice))
            {
                var assureurCode = await ResolveAssureurCodeAsync(assurance.Partenaire);
                assurance.NoPolice = await _numeroGeneratorService.GenerateNoPoliceLAsync(assureurCode);
            }

            if (string.IsNullOrWhiteSpace(assurance.NumeroCert))
            {
                assurance.NumeroCert = await _numeroGeneratorService.GenerateNumeroCertAsync();
            }

            assurance.DateDebut = signedAt;
            assurance.DateFin = signedAt.AddDays(durationDays);
        }

        assurance.ModifierLe = DateTime.UtcNow;

        await _assuranceRepository.UpdateAsync(assurance);

        var eventSignedAt = visa.ModifierLe ?? DateTime.UtcNow;
        await _publishEndpoint.Publish(new AssuranceSignedEvent
        {
            AssuranceId = assurance.Id,
            NoPolice = assurance.NoPolice,
            NumeroCert = assurance.NumeroCert,
            NoFacture = assurance.NoFacture,
            ImportateurNom = assurance.ImportateurNom,
            ImportateurNIU = assurance.ImportateurNIU,
            TypePartenaire = visa.TypePartenaire,
            Signataire = visa.Organisation,
            Decision = decision,
            VisaOK = visa.VisaOK,
            DateDebut = assurance.DateDebut,
            DateFin = assurance.DateFin,
            SignedAt = eventSignedAt
        }, cancellationToken);

        _logger.LogInformation(
            "Assurance signee. AssuranceId={AssuranceId} NoPolice={NoPolice} NumeroCert={NumeroCert} NoFacture={NoFacture} Organisation={Organisation} TypePartenaire={TypePartenaire} Decision={Decision} VisaOK={VisaOK} DateDebut={DateDebut} DateFin={DateFin}",
            assurance.Id,
            assurance.NoPolice,
            assurance.NumeroCert,
            assurance.NoFacture,
            visa.Organisation,
            visa.TypePartenaire,
            decision,
            visa.VisaOK,
            assurance.DateDebut,
            assurance.DateFin);

        return Unit.Value;
    }

    private static int ParseDurationDays(string? value)
    {
        if (!int.TryParse(value?.Trim(), out var days) || days <= 0)
        {
            throw new InvalidOperationException("La duree en jours est requise pour signer l'assurance.");
        }

        return days;
    }

    private async Task<string> ResolveAssureurCodeAsync(string? partenaire)
    {
        var normalized = NormalizeRequired(partenaire, "Assureur requis pour attribuer le numero de police.");

        if (normalized.StartsWith("SEG", StringComparison.OrdinalIgnoreCase))
        {
            var organisation = await _partenaireService.GetOrganisationAsync(normalized);
            var sigle = NormalizeOptional(organisation?.Sigle);
            if (sigle is null)
            {
                throw new InvalidOperationException($"Impossible de retrouver le code court de l'assureur {normalized}.");
            }

            return sigle;
        }

        return normalized;
    }

    private static string NormalizeRequired(string? value, string message)
    {
        var normalized = NormalizeOptional(value);
        if (normalized is null)
        {
            throw new InvalidOperationException(message);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
