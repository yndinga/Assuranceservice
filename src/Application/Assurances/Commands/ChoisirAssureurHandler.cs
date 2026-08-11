using AssuranceService.Application.Common;
using AssuranceService.Domain.Constants;
using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

public class ChoisirAssureurHandler : IRequestHandler<ChoisirAssureurCommand, Unit>
{
    private readonly IAssuranceRepository _assuranceRepository;

    public ChoisirAssureurHandler(IAssuranceRepository assuranceRepository)
    {
        _assuranceRepository = assuranceRepository;
    }

    public async Task<Unit> Handle(ChoisirAssureurCommand request, CancellationToken cancellationToken)
    {
        var assurance = await _assuranceRepository.GetByIdAsync(request.AssuranceId);
        if (assurance == null)
        {
            throw new InvalidOperationException($"Assurance {request.AssuranceId} introuvable.");
        }

        if (string.IsNullOrWhiteSpace(assurance.Intermediaire))
        {
            throw new InvalidOperationException("Cette assurance n'a pas ete envoyee a un intermediaire ; le choix d'assureur ne s'applique pas.");
        }

        if (!string.IsNullOrWhiteSpace(assurance.Partenaire))
        {
            throw new InvalidOperationException("Un assureur a deja ete choisi pour cette demande.");
        }

        if (!StatutAssuranceCodes.IsElabore(assurance.Etat))
        {
            throw new InvalidOperationException($"Seules les demandes au statut Elabore (42) peuvent recevoir un choix d'assureur. Statut actuel : {assurance.Etat}.");
        }

        if (string.IsNullOrWhiteSpace(request.Assureur))
        {
            throw new InvalidOperationException("Assureur invalide.");
        }

        assurance.Partenaire = request.Assureur.Trim();
        assurance.ModifierLe = DateTime.UtcNow;
        await _assuranceRepository.UpdateAsync(assurance);

        return Unit.Value;
    }
}

