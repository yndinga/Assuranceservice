using AssuranceService.Domain.Models.Referentiel;

namespace AssuranceService.Application.Common;

public interface IModuleRepository
{
    Task<Module?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
