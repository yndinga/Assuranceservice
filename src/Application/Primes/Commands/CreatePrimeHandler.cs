using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using MediatR;

namespace AssuranceService.Application.Primes.Commands;

public class CreatePrimeHandler : IRequestHandler<CreatePrimeCommand, Guid>
{
    private readonly IPrimeRepository _primeRepository;
    private readonly IAssuranceRepository _assuranceRepository;

    public CreatePrimeHandler(IPrimeRepository primeRepository, IAssuranceRepository assuranceRepository)
    {
        _primeRepository = primeRepository;
        _assuranceRepository = assuranceRepository;
    }

    public async Task<Guid> Handle(CreatePrimeCommand request, CancellationToken cancellationToken)
    {
        // Vérifier que l'assurance existe
        var assurance = await _assuranceRepository.GetByIdAsync(request.AssuranceId);
        if (assurance == null)
        {
            throw new ArgumentException($"Assurance with ID {request.AssuranceId} not found.");
        }

        var prime = new Prime
        {
            Taux = request.Taux,
            ValeurFCFA = request.ValeurFCFA,
            ValeurDevise = request.ValeurDevise,
            PrimeNette = request.PrimeNette,
            Accessoires = request.Accessoires,
            Taxe = request.Taxe,
            PrimeTotale = request.PrimeTotale,
            AssuranceId = request.AssuranceId,
            Statut = request.Statut,
            CreerPar = "System", // TODO: Utilisateur connecté
            ModifierPar = "System",
            CreerLe = DateTime.UtcNow
        };

        var createdPrime = await _primeRepository.CreateAsync(prime);
        return createdPrime.Id;
    }
}



