using AssuranceService.Application.Common;

namespace AssuranceService.Infrastructure.Messaging.Consumers;

internal static class ReferentielConsumerHelpers
{
    public static Task Insert<TLocal>(IReferentielSyncService sync, Guid id, string json, CancellationToken ct)
        where TLocal : class => sync.InsertFromJsonAsync<TLocal>(id, json, ct);

    public static Task Upsert<TLocal>(IReferentielSyncService sync, Guid id, string json, CancellationToken ct)
        where TLocal : class => sync.UpsertFromJsonAsync<TLocal>(id, json, ct);

    public static Task Delete<TLocal>(IReferentielSyncService sync, Guid id, CancellationToken ct)
        where TLocal : class => sync.DeleteAsync<TLocal>(id, ct);
}
