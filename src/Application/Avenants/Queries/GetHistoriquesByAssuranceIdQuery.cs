using AssuranceService.Application.Avenants.DTOs;
using MediatR;

namespace AssuranceService.Application.Avenants.Queries;

public record GetHistoriquesByAssuranceIdQuery(Guid AssuranceId) : IRequest<IReadOnlyList<HistoriqueLigneListeDto>>;
