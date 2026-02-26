using AssuranceService.Domain.Models;

namespace AssuranceService.Application.Common;

public interface IMarchandiseRepository
{
    Task<Marchandise?> GetByIdAsync(Guid id);
    Task<IEnumerable<Marchandise>> GetByAssuranceIdAsync(Guid assuranceId);
    Task<Marchandise> CreateAsync(Marchandise marchandise);
    Task<Marchandise> UpdateAsync(Marchandise marchandise);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}


