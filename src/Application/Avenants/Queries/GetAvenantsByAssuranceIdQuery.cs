using AssuranceService.Application.Avenants.DTOs;
using MediatR;

namespace AssuranceService.Application.Avenants.Queries;

public record GetAvenantsByAssuranceIdQuery(Guid AssuranceId) : IRequest<IReadOnlyList<AvenantListItemDto>>;
