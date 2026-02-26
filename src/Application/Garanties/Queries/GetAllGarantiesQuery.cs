using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Garanties.Queries;

public record GetAllGarantiesQuery : IRequest<IEnumerable<GarantieDto>>;



