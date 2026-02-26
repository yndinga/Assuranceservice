using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

public record StartAssuranceProcessCommand : IRequest<Guid>
{
    public Guid AssuranceId { get; init; }
}





