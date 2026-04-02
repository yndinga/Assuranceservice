using AssuranceService.Domain.Models;

namespace AssuranceService.Application.Common;

public interface IAvenantRepository
{
    Task<IReadOnlyList<Avenant>> ListByAssuranceIdAsync(Guid assuranceId, CancellationToken cancellationToken = default);

    Task<Avenant?> GetWithHistoriquesAsync(Guid assuranceId, Guid avenantId, CancellationToken cancellationToken = default);

    /// <summary>Toutes les lignes d’historique de la police (tous avenants), tri récent d’abord.</summary>
    Task<IReadOnlyList<Historique>> GetHistoriquesByAssuranceIdAsync(Guid assuranceId, CancellationToken cancellationToken = default);
}
