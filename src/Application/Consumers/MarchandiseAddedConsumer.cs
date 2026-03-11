using MassTransit;
using AssuranceService.Domain.Constants;
using AssuranceService.Domain.Events;
using AssuranceService.Application.Common;
using Microsoft.Extensions.Logging;

namespace AssuranceService.Application.Consumers;

public class MarchandiseAddedConsumer : IConsumer<MarchandiseAddedEvent>
{
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly ILogger<MarchandiseAddedConsumer> _logger;

    public MarchandiseAddedConsumer(IAssuranceRepository assuranceRepository, ILogger<MarchandiseAddedConsumer> logger)
    {
        _assuranceRepository = assuranceRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<MarchandiseAddedEvent> context)
    {
        _logger.LogInformation("Processing MarchandiseAddedEvent for Assurance {AssuranceId}", context.Message.AssuranceId);

        try
        {
            var assurance = await _assuranceRepository.GetByIdAsync(context.Message.AssuranceId);
            if (assurance == null)
            {
                _logger.LogWarning("Assurance {AssuranceId} not found", context.Message.AssuranceId);
                return;
            }

            // Mettre à jour le statut de l'assurance
            assurance.Statut = StatutAssuranceCodes.Elaboré;
            await _assuranceRepository.UpdateAsync(assurance);

            _logger.LogInformation("Assurance {AssuranceId} status updated to MarchandisesAdded", context.Message.AssuranceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MarchandiseAddedEvent for Assurance {AssuranceId}", context.Message.AssuranceId);
            throw;
        }
    }
}
