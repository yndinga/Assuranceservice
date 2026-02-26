using AssuranceService.Domain.Models;

namespace AssuranceService.Application.Common;

public interface IGarantieRepository
{
    Task<Garantie?> GetByIdAsync(Guid id);
    Task<IEnumerable<Garantie>> GetAllAsync();
    Task<Garantie> CreateAsync(Garantie garantie);
    Task<Garantie> UpdateAsync(Garantie garantie);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<Garantie>> GetActiveAsync();
}


