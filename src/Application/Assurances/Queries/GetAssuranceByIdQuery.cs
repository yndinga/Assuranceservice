using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Assurances.Queries;

public record GetAssuranceByIdQuery(Guid Id) : IRequest<AssuranceDetailDto?>;



