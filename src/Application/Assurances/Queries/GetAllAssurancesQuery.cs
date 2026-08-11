using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Assurances.Queries;

/// <summary>
/// Liste avec recherche (comme Laravel). Filtre par organisation connectée (code + type), fourni par le système
/// (en-têtes Gateway X-Organisation-Code / X-Organisation-Type), pas par l'utilisateur.
/// Pagination : gérée en interne (non exposée sur l'endpoint).
/// Aucun code organisation = administrateur (tout).
/// </summary>
public record GetAllAssurancesQuery(
    string? Search = null,
    string? OrganisationCode = null,
    string? OrganisationType = null
) : IRequest<PagedResult<AssuranceDto>>;



