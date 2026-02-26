using AssuranceService.Domain.Entities;

namespace AssuranceService.Application.Common;

public interface IPolicyRepository
{
    Task<Policy?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Policy policy, CancellationToken ct = default);
}
