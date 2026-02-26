using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

public record DeleteAssuranceCommand(Guid Id) : IRequest<Unit>;





