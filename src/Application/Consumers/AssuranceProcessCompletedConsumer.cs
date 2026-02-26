using MassTransit;
using AssuranceService.Domain.Events;
using AssuranceService.Application.Common;
using Microsoft.Extensions.Logging;

namespace AssuranceService.Application.Consumers;

public class AssuranceProcessCompletedConsumer : IConsumer<AssuranceProcessCompletedEvent>
{
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly ILogger<AssuranceProcessCompletedConsumer> _logger;

    public AssuranceProcessCompletedConsumer(IAssuranceRepository assuranceRepository, ILogger<AssuranceProcessCompletedConsumer> logger)
    {
        _assuranceRepository = assuranceRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AssuranceProcessCompletedEvent> context)
    {
        _logger.LogInformation("Processing AssuranceProcessCompletedEvent for Assurance {AssuranceId}", context.Message.AssuranceId);

        try
        {
            var assurance = await _assuranceRepository.GetByIdAsync(context.Message.AssuranceId);
            if (assurance == null)
            {
                _logger.LogWarning("Assurance {AssuranceId} not found", context.Message.AssuranceId);
                return;
            }

            // Mettre à jour le statut final de l'assurance
            assurance.Statut = context.Message.Statut;
            await _assuranceRepository.UpdateAsync(assurance);

            _logger.LogInformation("Assurance {AssuranceId} process completed with status {Status}", 
                context.Message.AssuranceId, context.Message.Statut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AssuranceProcessCompletedEvent for Assurance {AssuranceId}", context.Message.AssuranceId);
            throw;
        }
    }
}
