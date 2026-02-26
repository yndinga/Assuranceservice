using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using MediatR;

namespace AssuranceService.Application.Garanties.Commands;

public class UpdateGarantieHandler : IRequestHandler<UpdateGarantieCommand, bool>
{
    private readonly IGarantieRepository _garantieRepository;

    public UpdateGarantieHandler(IGarantieRepository garantieRepository)
    {
        _garantieRepository = garantieRepository;
    }

    public async Task<bool> Handle(UpdateGarantieCommand request, CancellationToken cancellationToken)
    {
        var garantie = await _garantieRepository.GetByIdAsync(request.Id);
        if (garantie == null)
            return false;

        garantie.Nom = request.NomGarantie;
        garantie.Taux = request.Taux;
        garantie.Accessoires = request.Accessoires;
        garantie.Actif = request.Actif;
        garantie.ModifierPar = "System";
        garantie.ModifierLe = DateTime.UtcNow;

        await _garantieRepository.UpdateAsync(garantie);
        return true;
    }
}
