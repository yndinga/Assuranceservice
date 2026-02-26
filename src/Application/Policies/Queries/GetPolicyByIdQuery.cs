using MediatR;
using AssuranceService.Domain.Entities;

namespace AssuranceService.Application.Policies.Queries;

public record GetPolicyByIdQuery(Guid Id) : IRequest<Policy?>;
