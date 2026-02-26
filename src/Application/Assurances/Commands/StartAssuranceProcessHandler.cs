using AssuranceService.Application.Common;
using AssuranceService.Domain.Events;
using MediatR;
using MassTransit;

namespace AssuranceService.Application.Assurances.Commands;

public class StartAssuranceProcessHandler : IRequestHandler<StartAssuranceProcessCommand, Guid>
{
    private readonly IAssuranceRepository _assuranceRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public StartAssuranceProcessHandler(IAssuranceRepository assuranceRepository, IPublishEndpoint publishEndpoint)
    {
        _assuranceRepository = assuranceRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(StartAssuranceProcessCommand request, CancellationToken cancellationToken)
    {
        var assurance = await _assuranceRepository.GetByIdAsync(request.AssuranceId);
        if (assurance == null)
        {
            throw new ArgumentException($"Assurance with ID {request.AssuranceId} not found.");
        }

        // Publier l'événement pour démarrer le processus SAGA
        await _publishEndpoint.Publish(new AssuranceProcessStartedEvent
        {
            AssuranceId = request.AssuranceId,
            NoPolice = assurance.NoPolice ?? string.Empty,
            TypeContrat = assurance.TypeContrat,
            StartedAt = DateTime.UtcNow
        }, cancellationToken);

        return request.AssuranceId;
    }
}



