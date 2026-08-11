using AssuranceService.Application.Common;
using AssuranceService.Domain.Constants;
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

    private static IQueryable<Assurance> WithDetails(IQueryable<Assurance> query) =>
        query
            .Include(a => a.Voyage)!
                .ThenInclude(v => v!.Maritime)
            .Include(a => a.Voyage)!
                .ThenInclude(v => v!.Aerien)
            .Include(a => a.Voyage)!
                .ThenInclude(v => v!.Routier)
            .Include(a => a.Voyage)!
                .ThenInclude(v => v!.Fluvial)
            .Include(a => a.Garantie)
            .Include(a => a.Declarations)
            .Include(a => a.Lignes);

    public async Task<Assurance?> GetByIdAsync(Guid id)
    {
        return await WithDetails(_context.Assurances)
            .Include(a => a.Primes)
            .Include(a => a.Visas)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Assurance?> GetByIdMinimalAsync(Guid id)
    {
        return await _context.Assurances
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<string?> GetStatutAsync(Guid id)
    {
        return await _context.Assurances
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => a.Etat)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Assurance>> GetAllAsync()
    {
        return await WithDetails(_context.Assurances)
            .Include(a => a.Primes)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Assurance> Items, int TotalCount)> GetPagedAsync(string? search, int page, int perPage, string? organisationCode, string? organisationType)
    {
        var query = _context.Assurances.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(organisationCode))
        {
            if (string.Equals(organisationType, TypePartenaireCodes.Assureur, StringComparison.OrdinalIgnoreCase))
                query = query.Where(a => a.Partenaire == organisationCode || a.OCRE == organisationCode);
            else if (TypePartenaireCodes.Intermediaires.Contains(organisationType ?? string.Empty))
                query = query.Where(a => a.Intermediaire == organisationCode || a.OCRE == organisationCode);
            else
                query = query.Where(a => a.OCRE == organisationCode || a.PCRE == organisationCode);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                (a.ImportateurNom != null && a.ImportateurNom.Contains(term)) ||
                (a.ImportateurNIU != null && a.ImportateurNIU.Contains(term)) ||
                a.NumeroAFI.Contains(term) ||
                (a.NoPolice != null && a.NoPolice.Contains(term)) ||
                (a.NumeroCert != null && a.NumeroCert.Contains(term)));
        }

        query = query.OrderByDescending(a => a.ModifierLe);

        var totalCount = await query.CountAsync();

        var items = await WithDetails(query)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Assurance>> GetByAssureurCodeAsync(string assureurCode)
    {
        return await WithDetails(_context.Assurances)
            .Include(a => a.Primes)
            .Where(a => a.Partenaire == assureurCode)
            .OrderByDescending(a => a.ModifierLe)
            .ToListAsync();
    }

    public async Task<IEnumerable<Assurance>> GetByIntermediaireCodeAsync(string intermediaireCode)
    {
        return await WithDetails(_context.Assurances)
            .Include(a => a.Primes)
            .Where(a => a.Intermediaire == intermediaireCode)
            .OrderByDescending(a => a.ModifierLe)
            .ToListAsync();
    }

    public Task<Assurance> CreateAsync(Assurance assurance)
    {
        if (assurance.Id == Guid.Empty)
            assurance.Id = Guid.NewGuid();
        if (string.IsNullOrWhiteSpace(assurance.NumeroAFI))
            assurance.NumeroAFI = Assurance.GenererNumeroAFI(assurance.Id);
        assurance.CreerLe = DateTime.UtcNow;
        assurance.ModifierLe = DateTime.UtcNow;

        _context.Assurances.Add(assurance);
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
        return await WithDetails(_context.Assurances)
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

    public async Task<IReadOnlyDictionary<Guid, AssuranceLineConsumption>> GetLineConsumptionsAsync(
        IReadOnlyCollection<Guid> sourceLineIds,
        Guid? excludeAssuranceId = null,
        CancellationToken cancellationToken = default)
    {
        if (sourceLineIds.Count == 0)
            return new Dictionary<Guid, AssuranceLineConsumption>();

        var refusedStatuses = new[] { StatutAssuranceCodes.Refuse, StatutAssuranceCodes.RefuseSecondaire };
        var consumptions = await _context.AssuranceLignes
            .AsNoTracking()
            .Where(l => sourceLineIds.Contains(l.SourceLigneDIId)
                        && !refusedStatuses.Contains(l.Assurance.Etat)
                        && (!excludeAssuranceId.HasValue || l.AssuranceId != excludeAssuranceId.Value))
            .GroupBy(l => l.SourceLigneDIId)
            .Select(group => new
            {
                SourceLineId = group.Key,
                Quantite = group.Sum(l => l.Quantite ?? 0m),
                MasseBrute = group.Sum(l => l.MasseBrute ?? 0m),
                MasseNette = group.Sum(l => l.MasseNette ?? 0m),
                Volume = group.Sum(l => l.Volume ?? 0m),
                ValeurDevise = group.Sum(l => l.ValeurDevise ?? 0m),
                ValeurXAF = group.Sum(l => l.ValeurXAF ?? 0m)
            })
            .ToListAsync(cancellationToken);

        return consumptions.ToDictionary(
            item => item.SourceLineId,
            item => new AssuranceLineConsumption(
                item.Quantite,
                item.MasseBrute,
                item.MasseNette,
                item.Volume,
                item.ValeurDevise,
                item.ValeurXAF));
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
