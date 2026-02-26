using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssuranceService.Infrastructure.Repositories;

public class VoyageRepository : IVoyageRepository
{
    private readonly AssuranceDbContext _context;

    public VoyageRepository(AssuranceDbContext context)
    {
        _context = context;
    }

    public Task<Voyage> CreateAsync(Voyage voyage)
    {
        voyage.Id = Guid.NewGuid();
        voyage.CreerLe = DateTime.UtcNow;
        voyage.ModifierLe = DateTime.UtcNow;
        _context.Voyages.Add(voyage);
        return Task.FromResult(voyage);
    }

    public async Task<Voyage?> GetByIdAsync(Guid id)
    {
        return await _context.Voyages
            .Include(v => v.Module)
            .Include(v => v.Maritime!)
                .ThenInclude(m => m.PortEmbarquement)
            .Include(v => v.Maritime!)
                .ThenInclude(m => m.PortDebarquement)
            .Include(v => v.Aerien)
            .Include(v => v.Routier)
            .Include(v => v.Fluvial)
                .ThenInclude(f => f!.PortEmbarquement)
            .Include(v => v.Fluvial)
                .ThenInclude(f => f!.PortDebarquement)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<Voyage>> GetByAssuranceIdAsync(Guid assuranceId)
    {
        return await _context.Voyages
            .Include(v => v.Module)
            .Include(v => v.Maritime!)
                .ThenInclude(m => m.PortEmbarquement)
            .Include(v => v.Maritime!)
                .ThenInclude(m => m.PortDebarquement)
            .Include(v => v.Aerien)
            .Include(v => v.Routier)
            .Include(v => v.Fluvial)
                .ThenInclude(f => f!.PortEmbarquement)
            .Include(v => v.Fluvial)
                .ThenInclude(f => f!.PortDebarquement)
            .Where(v => v.AssuranceId == assuranceId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Voyage>> GetAllAsync()
    {
        return await _context.Voyages
            .Include(v => v.Module)
            .Include(v => v.Maritime!)
                .ThenInclude(m => m.PortEmbarquement)
            .Include(v => v.Maritime!)
                .ThenInclude(m => m.PortDebarquement)
            .Include(v => v.Aerien)
            .Include(v => v.Routier)
            .Include(v => v.Fluvial)
                .ThenInclude(f => f!.PortEmbarquement)
            .Include(v => v.Fluvial)
                .ThenInclude(f => f!.PortDebarquement)
            .ToListAsync();
    }

    public async Task UpdateAsync(Voyage voyage)
    {
        voyage.ModifierLe = DateTime.UtcNow;
        _context.Voyages.Update(voyage);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var voyage = await _context.Voyages.FindAsync(id);
        if (voyage != null)
        {
            _context.Voyages.Remove(voyage);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task AddMaritimeAsync(Maritime maritime) => await _context.Maritimes.AddAsync(maritime);
    public async Task AddAerienAsync(Aerien aerien)       => await _context.Aeriens.AddAsync(aerien);
    public async Task AddRoutierAsync(Routier routier)    => await _context.Routiers.AddAsync(routier);
    public async Task AddFluvialAsync(Fluvial fluvial)    => await _context.Fluviaux.AddAsync(fluvial);
}
