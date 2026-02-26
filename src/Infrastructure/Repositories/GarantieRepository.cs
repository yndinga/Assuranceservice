using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssuranceService.Infrastructure.Repositories;

public class GarantieRepository : IGarantieRepository
{
    private readonly AssuranceDbContext _context;

    public GarantieRepository(AssuranceDbContext context)
    {
        _context = context;
    }

    public async Task<Garantie?> GetByIdAsync(Guid id)
    {
        return await _context.Garanties
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Garantie>> GetAllAsync()
    {
        return await _context.Garanties.ToListAsync();
    }

    public async Task<Garantie> CreateAsync(Garantie garantie)
    {
        garantie.Id = Guid.NewGuid();
        garantie.CreerLe = DateTime.UtcNow;
        garantie.ModifierLe = DateTime.UtcNow;

        _context.Garanties.Add(garantie);
        await _context.SaveChangesAsync();
        return garantie;
    }

    public async Task<Garantie> UpdateAsync(Garantie garantie)
    {
        garantie.ModifierLe = DateTime.UtcNow;
        _context.Garanties.Update(garantie);
        await _context.SaveChangesAsync();
        return garantie;
    }

    public async Task DeleteAsync(Guid id)
    {
        var garantie = await _context.Garanties.FindAsync(id);
        if (garantie != null)
        {
            _context.Garanties.Remove(garantie);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Garanties
            .AnyAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Garantie>> GetActiveAsync()
    {
        return await _context.Garanties
            .Where(g => g.Actif)
            .ToListAsync();
    }
}


