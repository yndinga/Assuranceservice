using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Assurances.Queries;

/// <summary>Liste des assurances envoyées à l'intermédiaire (courtier / agent général) — il peut les voir et signer.</summary>
public record GetAssurancesByIntermediaireQuery(Guid IntermediaireId) : IRequest<IEnumerable<AssuranceDto>>;
