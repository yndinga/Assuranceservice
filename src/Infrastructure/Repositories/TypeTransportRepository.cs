using AssuranceService.Application.Common;
using AssuranceService.Domain.Models.Referentiel;
using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssuranceService.Infrastructure.Repositories;

public class TypeTransportRepository : ITypeTransportRepository
{
    private readonly AssuranceDbContext _context;

    public TypeTransportRepository(AssuranceDbContext context) => _context = context;

    public async Task<TypeTransport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.TypeTransports.FindAsync([id], cancellationToken);
}
