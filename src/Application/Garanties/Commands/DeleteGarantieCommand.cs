using MediatR;

namespace AssuranceService.Application.Garanties.Commands;

public record DeleteGarantieCommand(Guid Id) : IRequest<bool>;
