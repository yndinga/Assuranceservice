using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Voyages.Queries;

public record GetVoyagesByAssuranceIdQuery(Guid AssuranceId) : IRequest<IEnumerable<VoyageDto>>;



