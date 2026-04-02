using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssuranceService.Infrastructure.Repositories;

public class AvenantRepository : IAvenantRepository
{
    private readonly AssuranceDbContext _context;

    public AvenantRepository(AssuranceDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Avenant>> ListByAssuranceIdAsync(Guid assuranceId, CancellationToken cancellationToken = default)
    {
        return await _context.Avenants
            .AsNoTracking()
            .Include(a => a.Historiques)
            .Where(a => a.AssuranceId == assuranceId)
            .OrderByDescending(a => a.CreerLe)
            .ToListAsync(cancellationToken);
    }

    public async Task<Avenant?> GetWithHistoriquesAsync(Guid assuranceId, Guid avenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Avenants
            .AsNoTracking()
            .Include(a => a.Historiques)
            .FirstOrDefaultAsync(a => a.AssuranceId == assuranceId && a.Id == avenantId, cancellationToken);
    }

    public async Task<IReadOnlyList<Historique>> GetHistoriquesByAssuranceIdAsync(Guid assuranceId, CancellationToken cancellationToken = default)
    {
        return await _context.Historiques
            .AsNoTracking()
            .Include(h => h.Avenant)
            .Where(h => h.AssuranceId == assuranceId)
            .OrderByDescending(h => h.CreerLe)
            .ToListAsync(cancellationToken);
    }
}
