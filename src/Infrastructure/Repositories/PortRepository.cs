using AssuranceService.Application.Common;
using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssuranceService.Infrastructure.Repositories;

public class PortRepository : IPortRepository
{
    private readonly AssuranceDbContext _context;

    public PortRepository(AssuranceDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Ports.AnyAsync(p => p.Id == id, cancellationToken);
    }
}
