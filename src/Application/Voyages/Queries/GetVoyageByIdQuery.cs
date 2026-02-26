using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Voyages.Queries;

public record GetVoyageByIdQuery(Guid Id) : IRequest<VoyageDto?>;



