using MassTransit;
using AssuranceService.Domain.Constants;
using AssuranceService.Domain.Events;
using AssuranceService.Application.Common;
using Microsoft.Extensions.Logging;

namespace AssuranceService.Application.Consumers;

public class AssuranceCreatedConsumer : IConsumer<AssuranceCreatedEvent>
{
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly ILogger<AssuranceCreatedConsumer> _logger;

    public AssuranceCreatedConsumer(IAssuranceRepository assuranceRepository, ILogger<AssuranceCreatedConsumer> logger)
    {
        _assuranceRepository = assuranceRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AssuranceCreatedEvent> context)
    {
        _logger.LogInformation("Processing AssuranceCreatedEvent for Assurance {AssuranceId}", context.Message.AssuranceId);

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

            _logger.LogInformation("Assurance {AssuranceId} status updated to Created", context.Message.AssuranceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AssuranceCreatedEvent for Assurance {AssuranceId}", context.Message.AssuranceId);
            throw;
        }
    }
}
