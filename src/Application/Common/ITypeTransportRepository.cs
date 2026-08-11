using AssuranceService.Domain.Models.Referentiel;

namespace AssuranceService.Application.Common;

public interface ITypeTransportRepository
{
    Task<TypeTransport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
