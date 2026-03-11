using AssuranceService.Application.Common;
using AssuranceService.Domain.Models.Referentiel;
using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssuranceService.Infrastructure.Repositories;

public class DeviseRepository : IDeviseRepository
{
    private readonly AssuranceDbContext _context;

    public DeviseRepository(AssuranceDbContext context)
    {
        _context = context;
    }

    public async Task<Devise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Devises.FindAsync(new object[] { id }, cancellationToken);
    }
}
