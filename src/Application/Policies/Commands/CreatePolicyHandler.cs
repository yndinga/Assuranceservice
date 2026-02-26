using AssuranceService.Application.Common;
using AssuranceService.Domain.Entities;
using MediatR;

namespace AssuranceService.Application.Policies.Commands;

public class CreatePolicyHandler : IRequestHandler<CreatePolicyCommand, Guid>
{
    private readonly IPolicyRepository _repo;

    public CreatePolicyHandler(IPolicyRepository repo) => _repo = repo;

    public async Task<Guid> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = new Policy(request.Number, request.CustomerId, request.Premium, request.StartDate, request.EndDate);
        await _repo.AddAsync(policy, cancellationToken);
        return policy.Id;
    }
}
