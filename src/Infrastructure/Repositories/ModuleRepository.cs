using AssuranceService.Application.Common;
using AssuranceService.Domain.Models.Referentiel;
using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssuranceService.Infrastructure.Repositories;

public class ModuleRepository : IModuleRepository
{
    private readonly AssuranceDbContext _context;

    public ModuleRepository(AssuranceDbContext context)
    {
        _context = context;
    }

    public async Task<Module?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Modules.FindAsync(new object[] { id }, cancellationToken);
    }
}
