using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using AssuranceService.Domain.Events;
using MediatR;
using MassTransit;

namespace AssuranceService.Application.Marchandises.Commands;

public class CreateMarchandiseHandler : IRequestHandler<CreateMarchandiseCommand, Guid>
{
    private readonly IMarchandiseRepository _marchandiseRepository;
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly IPrimeCalculatorService _primeCalculatorService;
    private readonly IPrimeRepository _primeRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateMarchandiseHandler(
        IMarchandiseRepository marchandiseRepository,
        IAssuranceRepository assuranceRepository,
        IPrimeCalculatorService primeCalculatorService,
        IPrimeRepository primeRepository,
        IPublishEndpoint publishEndpoint)
    {
        _marchandiseRepository = marchandiseRepository;
        _assuranceRepository = assuranceRepository;
        _primeCalculatorService = primeCalculatorService;
        _primeRepository = primeRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreateMarchandiseCommand request, CancellationToken cancellationToken)
    {
        // Vérifier que l'assurance existe
        var assurance = await _assuranceRepository.GetByIdAsync(request.AssuranceId);
        if (assurance == null)
        {
            throw new ArgumentException($"Assurance with ID {request.AssuranceId} not found.");
        }

        // Vérifier que l'assurance est au statut "Elaborer"
        if (assurance.Statut != "Elaborer")
        {
            throw new InvalidOperationException($"Impossible d'ajouter une marchandise. L'assurance doit être au statut 'Elaborer'. Statut actuel: {assurance.Statut}");
        }

        var marchandise = new Marchandise
        {
            Designation = request.Designation,
            Nature = request.Nature,
            Specificites = request.Specificites,
            Conditionnement = request.Conditionnement,
            Description = request.Description,
            AssuranceId = request.AssuranceId,
            ValeurFCFA = request.Valeur,
            ValeurDevise = request.ValeurDevise,
            Devise = request.Devise ?? "XOF",
            MasseBrute = request.MasseBrute,
            UniteStatistique = request.UniteStatistique ?? string.Empty,
            Marque = request.Marque,
            CreerPar = "System",
            ModifierPar = "System",
            CreerLe = DateTime.UtcNow
        };

        var createdMarchandise = await _marchandiseRepository.CreateAsync(marchandise);

        // Recalculer et enregistrer la Prime (formule : ValeurFCFA = ValeurDevise × TauxChange, PrimeNette = ValeurFCFA × Taux, Taxe 15%, PrimeTotale = PrimeNette + Taxe + Accessoires)
        var marchandises = await _marchandiseRepository.GetByAssuranceIdAsync(request.AssuranceId);
        var valeurTotaleDevise = marchandises.Sum(m => m.ValeurDevise ?? 0);
        var devise = marchandises
            .Where(m => !string.IsNullOrWhiteSpace(m.Devise))
            .Select(m => m.Devise)
            .FirstOrDefault() ?? "XOF";

        if (valeurTotaleDevise > 0 && assurance.GarantieId.HasValue)
        {
            var primeCalculation = await _primeCalculatorService.CalculerPrimeAsync(new PrimeCalculationRequest
            {
                AssuranceId = request.AssuranceId,
                GarantieId = assurance.GarantieId.Value,
                ValeurDevise = valeurTotaleDevise,
                Devise = devise
            });

            var existingPrimes = await _primeRepository.GetByAssuranceIdAsync(request.AssuranceId);
            var existingPrime = existingPrimes.FirstOrDefault();

            if (existingPrime != null)
            {
                existingPrime.Taux = primeCalculation.Taux.ToString();
                existingPrime.ValeurFCFA = primeCalculation.ValeurFCFA;
                existingPrime.ValeurDevise = valeurTotaleDevise;
                existingPrime.PrimeNette = primeCalculation.PrimeNette;
                existingPrime.Accessoires = (double)primeCalculation.Accessoires;
                existingPrime.Taxe = primeCalculation.Taxe;
                existingPrime.PrimeTotale = primeCalculation.PrimeTotale;
                existingPrime.Statut = "Calculée";
                existingPrime.ModifierLe = DateTime.UtcNow;
                await _primeRepository.UpdateAsync(existingPrime);
            }
            else
            {
                var prime = new Prime
                {
                    AssuranceId = request.AssuranceId,
                    Taux = primeCalculation.Taux.ToString(),
                    ValeurFCFA = primeCalculation.ValeurFCFA,
                    ValeurDevise = valeurTotaleDevise,
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
            }
        }

        // Publier l'événement pour le SAGA
        await _publishEndpoint.Publish(new MarchandiseAddedEvent
        {
            MarchandiseId = createdMarchandise.Id,
            AssuranceId = request.AssuranceId,
            Designation = request.Designation,
            Valeur = request.Valeur,
            Conditionnement = request.Conditionnement,
            AddedAt = DateTime.UtcNow
        }, cancellationToken);

        return createdMarchandise.Id;
    }
}
