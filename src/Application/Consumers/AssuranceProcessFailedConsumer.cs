using MassTransit;
using AssuranceService.Domain.Events;
using AssuranceService.Application.Common;
using Microsoft.Extensions.Logging;

namespace AssuranceService.Application.Consumers;

public class AssuranceProcessFailedConsumer : IConsumer<AssuranceProcessFailedEvent>
{
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly ILogger<AssuranceProcessFailedConsumer> _logger;

    public AssuranceProcessFailedConsumer(IAssuranceRepository assuranceRepository, ILogger<AssuranceProcessFailedConsumer> logger)
    {
        _assuranceRepository = assuranceRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AssuranceProcessFailedEvent> context)
    {
        _logger.LogError("Processing AssuranceProcessFailedEvent for Assurance {AssuranceId}: {ErrorMessage}", 
            context.Message.AssuranceId, context.Message.ErrorMessage);

        try
        {
            var assurance = await _assuranceRepository.GetByIdAsync(context.Message.AssuranceId);
            if (assurance == null)
            {
                _logger.LogWarning("Assurance {AssuranceId} not found", context.Message.AssuranceId);
                return;
            }

            // Mettre à jour le statut de l'assurance en échec
            assurance.Statut = "Failed";
            await _assuranceRepository.UpdateAsync(assurance);

            _logger.LogError("Assurance {AssuranceId} process failed: {ErrorMessage} (Code: {ErrorCode})", 
                context.Message.AssuranceId, context.Message.ErrorMessage, context.Message.ErrorCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AssuranceProcessFailedEvent for Assurance {AssuranceId}", context.Message.AssuranceId);
            throw;
        }
    }
}
