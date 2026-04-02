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
            .Include(a => a.Maritime)
            .Include(a => a.Aerien)
            .Include(a => a.Routier)
            .Include(a => a.Fluvial)
            .Include(a => a.Primes)
            .Include(a => a.Visas)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Assurance?> GetByIdMinimalAsync(Guid id)
    {
        return await _context.Assurances
            .Include(a => a.Garantie)
            .AsNoTracking()
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
            .Include(a => a.Maritime)
            .Include(a => a.Aerien)
            .Include(a => a.Routier)
            .Include(a => a.Fluvial)
            .Include(a => a.Primes)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Assurance> Items, int TotalCount)> GetPagedAsync(string? search, int page, int perPage, string? ocre, Guid? intermediaireId, Guid? assureurId)
    {
        var query = _context.Assurances.AsNoTracking();

        // Filtres rôle (comme Laravel) : un seul appliqué, sinon admin = tout
        if (!string.IsNullOrWhiteSpace(ocre))
            query = query.Where(a => a.OCRE == ocre);
        else if (intermediaireId.HasValue)
            query = query.Where(a => a.IntermediaireId == intermediaireId.Value);
        else if (assureurId.HasValue)
            query = query.Where(a => a.AssureurId == assureurId.Value);

        // Recherche : Nom (ImportateurNom), NIU, NoPolice, NumeroCert
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                (a.ImportateurNom != null && a.ImportateurNom.Contains(term)) ||
                (a.ImportateurNIU != null && a.ImportateurNIU.Contains(term)) ||
                (a.NoPolice != null && a.NoPolice.Contains(term)) ||
                (a.NumeroCert != null && a.NumeroCert.Contains(term)));
        }

        query = query.OrderByDescending(a => a.ModifierLe);

        var totalCount = await query.CountAsync();

        var items = await query
            .Include(a => a.Garantie)
            .Include(a => a.Maritime)
            .Include(a => a.Aerien)
            .Include(a => a.Routier)
            .Include(a => a.Fluvial)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Assurance>> GetByAssureurIdAsync(Guid assureurId)
    {
        return await _context.Assurances
            .Include(a => a.Garantie)
            .Include(a => a.Maritime)
            .Include(a => a.Aerien)
            .Include(a => a.Routier)
            .Include(a => a.Fluvial)
            .Include(a => a.Primes)
            .Where(a => a.AssureurId == assureurId)
            .OrderByDescending(a => a.ModifierLe)
            .ToListAsync();
    }

    public async Task<IEnumerable<Assurance>> GetByIntermediaireIdAsync(Guid intermediaireId)
    {
        return await _context.Assurances
            .Include(a => a.Garantie)
            .Include(a => a.Maritime)
            .Include(a => a.Aerien)
            .Include(a => a.Routier)
            .Include(a => a.Fluvial)
            .Include(a => a.Primes)
            .Where(a => a.IntermediaireId == intermediaireId)
            .OrderByDescending(a => a.ModifierLe)
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
            .Include(a => a.Maritime)
            .Include(a => a.Aerien)
            .Include(a => a.Routier)
            .Include(a => a.Fluvial)
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
    
    public async Task AddVisaAssuranceAsync(VisaAssurance visaAssurance)
    {
        await _context.VisaAssurances.AddAsync(visaAssurance);
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}


