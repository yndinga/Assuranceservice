using AssuranceService.Domain.Models;

namespace AssuranceService.Application.Common;

public interface IVoyageRepository
{
    Task<Voyage> CreateAsync(Voyage voyage);
    Task<Voyage?> GetByIdAsync(Guid id);
    Task<IEnumerable<Voyage>> GetByAssuranceIdAsync(Guid assuranceId);
    Task<IEnumerable<Voyage>> GetAllAsync();
    Task UpdateAsync(Voyage voyage);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();

    // Tables transport enfants (même pattern que DeclarationImportationService)
    Task AddMaritimeAsync(Maritime maritime);
    Task AddAerienAsync(Aerien aerien);
    Task AddRoutierAsync(Routier routier);
    Task AddFluvialAsync(Fluvial fluvial);
}
