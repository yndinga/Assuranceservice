using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Assurances.Queries;

/// <summary>Liste des assurances où l'assureur (maison d'assurance) est désigné — il peut les signer.</summary>
public record GetAssurancesByAssureurQuery(string AssureurCode) : IRequest<IEnumerable<AssuranceDto>>;
