namespace AssuranceService.Application.Common;

public interface IReferentielSyncService
{
    Task InsertFromJsonAsync<TEntity>(Guid id, string payloadJson, CancellationToken ct = default)
        where TEntity : class;

    Task UpsertFromJsonAsync<TEntity>(Guid id, string payloadJson, CancellationToken ct = default)
        where TEntity : class;

    Task DeleteAsync<TEntity>(Guid id, CancellationToken ct = default)
        where TEntity : class;

    Task InsertPaysAsync(Guid id, string code, string nom, bool actif, CancellationToken ct = default);

    Task UpsertPaysAsync(Guid id, string code, string nom, bool actif, CancellationToken ct = default);

    Task DeletePaysAsync(Guid id, CancellationToken ct = default);

    Task UpsertPaysFromJsonAsync(Guid id, string payloadJson, CancellationToken ct = default);
}
