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

    public async Task<Guid?> GetIdByCodeAsync(string? code, string? type = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var normalizedCode = code.Trim().ToUpper();
        var query = _context.Ports.AsNoTracking()
            .Where(p => p.Code.ToUpper() == normalizedCode);

        if (!string.IsNullOrWhiteSpace(type))
        {
            var normalizedType = type.Trim().ToUpper();
            query = query.Where(p =>
                (p.Type != null && p.Type.ToUpper() == normalizedType) ||
                p.Module.ToUpper() == normalizedType);
        }

        return await query
            .OrderByDescending(p => p.Actif)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
