using AssuranceService.Application.Common;
using AssuranceService.Domain.Entities;
using MediatR;

namespace AssuranceService.Application.Policies.Queries;

public class GetPolicyByIdHandler : IRequestHandler<GetPolicyByIdQuery, Policy?>
{
    private readonly IPolicyRepository _repo;
    public GetPolicyByIdHandler(IPolicyRepository repo) => _repo = repo;

    public Task<Policy?> Handle(GetPolicyByIdQuery request, CancellationToken cancellationToken)
        => _repo.GetByIdAsync(request.Id, cancellationToken);
}
