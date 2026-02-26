using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Garanties.Queries;

public record GetGarantieByIdQuery(Guid Id) : IRequest<GarantieDto?>;
