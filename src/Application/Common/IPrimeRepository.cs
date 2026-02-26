using AssuranceService.Domain.Models;

namespace AssuranceService.Application.Common;

public interface IPrimeRepository
{
    Task<Prime?> GetByIdAsync(Guid id);
    Task<IEnumerable<Prime>> GetByAssuranceIdAsync(Guid assuranceId);
    Task<Prime> CreateAsync(Prime prime);
    Task<Prime> UpdateAsync(Prime prime);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}


