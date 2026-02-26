using AssuranceService.Application.Common;
using AssuranceService.Domain.Entities;
using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssuranceService.Infrastructure.Repositories;

public class PolicyRepository : IPolicyRepository
{
    private readonly AssuranceDbContext _db;
    public PolicyRepository(AssuranceDbContext db) => _db = db;

    public async Task<Policy?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Policies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Policy policy, CancellationToken ct = default)
    {
        await _db.Policies.AddAsync(policy, ct);
        await _db.SaveChangesAsync(ct);
    }
}
