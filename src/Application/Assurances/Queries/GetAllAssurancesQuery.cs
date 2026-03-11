using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Assurances.Queries;

/// <summary>
/// Liste avec recherche (comme Laravel). Filtres rôle (OCRE, IntermediaireId, AssureurId) fournis par le système (en-têtes), pas par l'utilisateur.
/// Pagination : gérée en interne (non exposée sur l'endpoint).
/// Si aucun identifiant utilisateur en en-tête = administrateur (tout).
/// </summary>
public record GetAllAssurancesQuery(
    string? Search = null,
    string? Ocre = null,
    Guid? IntermediaireId = null,
    Guid? AssureurId = null
) : IRequest<PagedResult<AssuranceDto>>;



