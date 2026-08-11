using AssuranceService.Application.Common;
using AssuranceService.Domain.Constants;
using AssuranceService.Domain.Models;
using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

public class UpdateAssuranceHandler : IRequestHandler<UpdateAssuranceCommand, Unit>
{
    private readonly IAssuranceRepository _assuranceRepository;

    public UpdateAssuranceHandler(IAssuranceRepository assuranceRepository)
    {
        _assuranceRepository = assuranceRepository;
    }

    public async Task<Unit> Handle(UpdateAssuranceCommand request, CancellationToken cancellationToken)
    {
        var assurance = await _assuranceRepository.GetByIdAsync(request.Id);
        if (assurance == null)
        {
            throw new ArgumentException($"Assurance with ID {request.Id} not found.");
        }

        if (StatutAssuranceCodes.IsVisaDemande(assurance.Etat) || StatutAssuranceCodes.IsModificationSoumise(assurance.Etat) || StatutAssuranceCodes.IsRefuse(assurance.Etat))
        {
            throw new InvalidOperationException("L'assurance n'est pas editable dans son statut actuel.");
        }

        assurance.NoPolice = request.NoPolice;
        assurance.NumeroCert = request.NumeroCert;
        assurance.ImportateurNom = request.Importateur;
        assurance.TypeContrat = request.TypeContrat;
        assurance.Duree = request.Duree;
        assurance.DureeJours = ParseDurationDays(request.Duree);
        // Le statut est pilote par le workflow. L'edition conserve 42 ou 66 jusqu'a la soumission.

        var modeDeTransport = ModeDeTransportCodes.Normalize(request.ModeDeTransport);
        assurance.ModeDeTransport = modeDeTransport;

        if (assurance.Voyage == null)
        {
            assurance.Voyage = new Voyage
            {
                AssuranceId = assurance.Id,
                NomTransporteur = request.NomTransporteur ?? string.Empty,
                PaysProvenance = string.Empty,
                PaysDestination = string.Empty,
                CreerLe = DateTime.UtcNow
            };
        }
        else
        {
            assurance.Voyage.NomTransporteur = request.NomTransporteur ?? assurance.Voyage.NomTransporteur;
        }

        assurance.Voyage.ApplyNavireDetails(modeDeTransport, request.NomNavire, request.TypeNavire);

        assurance.GarantieId = ParseOptionalGuid(request.Garantie) ?? assurance.GarantieId;
        assurance.Partenaire = NormalizeOptional(request.Assureur) ?? assurance.Partenaire;
        assurance.Intermediaire = NormalizeOptional(request.Intermediaire);
        assurance.TypePartenaire = NormalizeTypePartenaire(request.TypePartenaire);
        assurance.ModifierPar = request.ModifierPar;

        await _assuranceRepository.UpdateAsync(assurance);
        return Unit.Value;
    }

    private static Guid? ParseOptionalGuid(string? value)
    {
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static string NormalizeTypePartenaire(string? value)
    {
        var normalized = NormalizeOptional(value)?.ToUpperInvariant();
        return normalized is TypePartenaireCodes.Courtier or TypePartenaireCodes.AgentGeneral
            ? normalized
            : TypePartenaireCodes.Assureur;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static int? ParseDurationDays(string? value)
    {
        return int.TryParse(value?.Trim(), out var days) && days > 0 ? days : null;
    }
}


