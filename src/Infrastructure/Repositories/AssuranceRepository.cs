using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssuranceService.Infrastructure.Repositories;

public class AssuranceRepository : IAssuranceRepository
{
    private readonly AssuranceDbContext _context;

    public AssuranceRepository(AssuranceDbContext context)
    {
        _context = context;
    }

    public async Task<Assurance?> GetByIdAsync(Guid id)
    {
        return await _context.Assurances
            .Include(a => a.Garantie)
            .Include(a => a.Marchandises)
            .Include(a => a.Primes)
            .Include(a => a.Visas)
            .Include(a => a.Voyage!)
                .ThenInclude(v => v.Module)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<string?> GetStatutAsync(Guid id)
    {
        return await _context.Assurances
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => a.Statut)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Assurance>> GetAllAsync()
    {
        return await _context.Assurances
            .Include(a => a.Garantie)
            .Include(a => a.Marchandises)
            .Include(a => a.Primes)
            .ToListAsync();
    }

    public Task<Assurance> CreateAsync(Assurance assurance)
    {
        assurance.Id = Guid.NewGuid();
        assurance.CreerLe = DateTime.UtcNow;
        assurance.ModifierLe = DateTime.UtcNow;

        _context.Assurances.Add(assurance);
        // Do not save here to allow adding transport modes; SaveChanges is called after
        return Task.FromResult(assurance);
    }

    public async Task<Assurance> UpdateAsync(Assurance assurance)
    {
        assurance.ModifierLe = DateTime.UtcNow;
        _context.Assurances.Update(assurance);
        await _context.SaveChangesAsync();
        return assurance;
    }

    public async Task DeleteAsync(Guid id)
    {
        var assurance = await _context.Assurances.FindAsync(id);
        if (assurance != null)
        {
            _context.Assurances.Remove(assurance);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Assurances
            .AnyAsync(a => a.Id == id);
    }

    public async Task<Assurance?> GetByNoPoliceAsync(string noPolice)
    {
        return await _context.Assurances
            .Include(a => a.Garantie)
            .Include(a => a.Marchandises)
            .Include(a => a.Primes)
            .FirstOrDefaultAsync(a => a.NoPolice == noPolice);
    }

    public async Task<string?> GetLastNumeroCertAsync()
    {
        return await _context.Assurances
            .AsNoTracking()
            .Where(a => !string.IsNullOrEmpty(a.NumeroCert) && a.NumeroCert.Length >= 10)
            .OrderByDescending(a => a.NumeroCert)
            .Select(a => a.NumeroCert)
            .FirstOrDefaultAsync();
    }
    
    public async Task AddVoyageAsync(Voyage voyage)
    {
        await _context.Voyages.AddAsync(voyage);
    }

    public async Task AddVisaAssuranceAsync(VisaAssurance visaAssurance)
    {
        await _context.VisaAssurances.AddAsync(visaAssurance);
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}


