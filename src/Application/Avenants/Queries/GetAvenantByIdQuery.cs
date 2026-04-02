using AssuranceService.Application.Avenants.DTOs;
using MediatR;

namespace AssuranceService.Application.Avenants.Queries;

public record GetAvenantByIdQuery(Guid AssuranceId, Guid AvenantId) : IRequest<AvenantDetailDto?>;
