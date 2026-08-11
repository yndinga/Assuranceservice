using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssuranceService.Infrastructure.Repositories;

public class PrimeRepository : IPrimeRepository
{
    private readonly AssuranceDbContext _context;

    public PrimeRepository(AssuranceDbContext context)
    {
        _context = context;
    }

    public async Task<Prime?> GetByIdAsync(Guid id)
    {
        return await _context.Primes
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Prime>> GetByAssuranceIdAsync(Guid assuranceId)
    {
        return await _context.Primes
            .Where(p => p.AssuranceId == assuranceId)
            .ToListAsync();
    }

    public async Task<Prime> CreateAsync(Prime prime)
    {
        var existing = await _context.Primes
            .OrderByDescending(item => item.ModifierLe ?? item.CreerLe)
            .FirstOrDefaultAsync(item => item.AssuranceId == prime.AssuranceId);
        if (existing is not null)
        {
            existing.Taux = prime.Taux;
            existing.ValeurFCFA = prime.ValeurFCFA;
            existing.ValeurDevise = prime.ValeurDevise;
            existing.PrimeNette = prime.PrimeNette;
            existing.Accessoires = prime.Accessoires;
            existing.Taxe = prime.Taxe;
            existing.PrimeTotale = prime.PrimeTotale;
            existing.Statut = prime.Statut;
            existing.ModifierPar = prime.ModifierPar;
            existing.ModifierLe = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

        prime.Id = Guid.NewGuid();
        prime.CreerLe = DateTime.UtcNow;
        prime.ModifierLe = DateTime.UtcNow;

        _context.Primes.Add(prime);
        await _context.SaveChangesAsync();
        return prime;
    }

    public async Task<Prime> UpdateAsync(Prime prime)
    {
        prime.ModifierLe = DateTime.UtcNow;
        _context.Primes.Update(prime);
        await _context.SaveChangesAsync();
        return prime;
    }

    public async Task DeleteAsync(Guid id)
    {
        var prime = await _context.Primes.FindAsync(id);
        if (prime != null)
        {
            _context.Primes.Remove(prime);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Primes
            .AnyAsync(p => p.Id == id);
    }
}

