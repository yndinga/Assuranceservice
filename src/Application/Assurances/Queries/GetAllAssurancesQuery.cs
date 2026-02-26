using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Assurances.Queries;

public record GetAllAssurancesQuery : IRequest<IEnumerable<AssuranceDto>>;



