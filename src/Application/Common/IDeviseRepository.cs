using AssuranceService.Domain.Models.Referentiel;

namespace AssuranceService.Application.Common;

public interface IDeviseRepository
{
    Task<Devise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
