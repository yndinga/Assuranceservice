using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using MediatR;

namespace AssuranceService.Application.Garanties.Commands;

public class CreateGarantieHandler : IRequestHandler<CreateGarantieCommand, Guid>
{
    private readonly IGarantieRepository _garantieRepository;

    public CreateGarantieHandler(IGarantieRepository garantieRepository)
    {
        _garantieRepository = garantieRepository;
    }

    public async Task<Guid> Handle(CreateGarantieCommand request, CancellationToken cancellationToken)
    {
        var garantie = new Garantie
        {
            Nom = request.NomGarantie,
            Taux = request.Taux,
            Accessoires = request.Accessoires,
            Actif = request.Actif,
            CreerPar = "System", // TODO: Utilisateur connecté
            ModifierPar = "System",
            CreerLe = DateTime.UtcNow
        };

        var createdGarantie = await _garantieRepository.CreateAsync(garantie);
        return createdGarantie.Id;
    }
}



