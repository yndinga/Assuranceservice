using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssuranceService.Infrastructure.Repositories;

public class MarchandiseRepository : IMarchandiseRepository
{
    private readonly AssuranceDbContext _context;

    public MarchandiseRepository(AssuranceDbContext context)
    {
        _context = context;
    }

    public async Task<Marchandise?> GetByIdAsync(Guid id)
    {
        return await _context.Marchandises
            .Include(m => m.Assurance)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Marchandise>> GetByAssuranceIdAsync(Guid assuranceId)
    {
        return await _context.Marchandises
            .Include(m => m.Assurance)
            .Where(m => m.AssuranceId == assuranceId)
            .ToListAsync();
    }

    public async Task<Marchandise> CreateAsync(Marchandise marchandise)
    {
        marchandise.Id = Guid.NewGuid();
        marchandise.CreerLe = DateTime.UtcNow;
        marchandise.ModifierLe = DateTime.UtcNow;

        _context.Marchandises.Add(marchandise);
        await _context.SaveChangesAsync();
        return marchandise;
    }

    public async Task<Marchandise> UpdateAsync(Marchandise marchandise)
    {
        marchandise.ModifierLe = DateTime.UtcNow;
        _context.Marchandises.Update(marchandise);
        await _context.SaveChangesAsync();
        return marchandise;
    }

    public async Task DeleteAsync(Guid id)
    {
        var marchandise = await _context.Marchandises.FindAsync(id);
        if (marchandise != null)
        {
            _context.Marchandises.Remove(marchandise);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Marchandises
            .AnyAsync(m => m.Id == id);
    }
}


