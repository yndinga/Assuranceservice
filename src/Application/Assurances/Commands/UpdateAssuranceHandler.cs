using AssuranceService.Application.Common;
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

        assurance.NoPolice = request.NoPolice;
        assurance.NumeroCert = request.NumeroCert;
        assurance.ImportateurNom = request.Importateur;
        assurance.DateDebut = request.DateDebut;
        assurance.DateFin = request.DateFin;
        assurance.TypeContrat = request.TypeContrat;
        assurance.Duree = request.Duree;
        assurance.Statut = request.Statut;
        assurance.Module = request.Module;
        assurance.GarantieId = request.GarantieId;
        assurance.AssureurId = request.AssureurId;
        assurance.IntermediaireId = request.IntermediaireId;
        assurance.ModifierPar = request.ModifierPar;

        await _assuranceRepository.UpdateAsync(assurance);
        return Unit.Value;
    }
}



