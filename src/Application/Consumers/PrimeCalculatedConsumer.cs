using MassTransit;
using AssuranceService.Domain.Constants;
using AssuranceService.Domain.Events;
using AssuranceService.Application.Common;
using Microsoft.Extensions.Logging;

namespace AssuranceService.Application.Consumers;

public class PrimeCalculatedConsumer : IConsumer<PrimeCalculatedEvent>
{
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly IPrimeRepository _primeRepository;
    private readonly ILogger<PrimeCalculatedConsumer> _logger;

    public PrimeCalculatedConsumer(
        IAssuranceRepository assuranceRepository, 
        IPrimeRepository primeRepository,
        ILogger<PrimeCalculatedConsumer> logger)
    {
        _assuranceRepository = assuranceRepository;
        _primeRepository = primeRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PrimeCalculatedEvent> context)
    {
        _logger.LogInformation("Processing PrimeCalculatedEvent for Assurance {AssuranceId}", context.Message.AssuranceId);

        try
        {
            var assurance = await _assuranceRepository.GetByIdAsync(context.Message.AssuranceId);
            if (assurance == null)
            {
                _logger.LogWarning("Assurance {AssuranceId} not found", context.Message.AssuranceId);
                return;
            }

            // Créer la prime
            var prime = new AssuranceService.Domain.Models.Prime
            {
                AssuranceId = context.Message.AssuranceId,
                ValeurFCFA = context.Message.ValeurFCFA,
                ValeurDevise = context.Message.ValeurDevise,
                PrimeNette = context.Message.PrimeNette,
                PrimeTotale = context.Message.PrimeTotale,
                Statut = context.Message.Statut,
                CreerPar = "System",
                ModifierPar = "System"
            };

            await _primeRepository.CreateAsync(prime);

            // Mettre à jour le statut de l'assurance
            assurance.Statut = StatutAssuranceCodes.Elaboré;
            await _assuranceRepository.UpdateAsync(assurance);

            _logger.LogInformation("Prime created and Assurance {AssuranceId} status updated to PrimeCalculated", context.Message.AssuranceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PrimeCalculatedEvent for Assurance {AssuranceId}", context.Message.AssuranceId);
            throw;
        }
    }
}
