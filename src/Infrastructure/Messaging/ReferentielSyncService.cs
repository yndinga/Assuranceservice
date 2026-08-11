using System.Text.Json;
using AssuranceService.Application.Common;
using AssuranceService.Domain.Models.Commons;
using AssuranceService.Domain.Models.Referentiel;
using AssuranceService.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace AssuranceService.Infrastructure.Messaging;

public sealed class ReferentielSyncService : IReferentielSyncService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AssuranceDbContext _db;
    private readonly ILogger<ReferentielSyncService> _logger;

    public ReferentielSyncService(AssuranceDbContext db, ILogger<ReferentielSyncService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task InsertFromJsonAsync<TEntity>(Guid id, string payloadJson, CancellationToken ct = default)
        where TEntity : class
    {
        if (await _db.Set<TEntity>().FindAsync([id], ct) is not null)
        {
            _logger.LogDebug("Référentiel déjà présent, Created ignoré : {Entity} ({Id})", typeof(TEntity).Name, id);
            return;
        }

        var entity = Deserialize<TEntity>(payloadJson);
        SetEntityId(entity, id);
        NormalizeEntity(entity);
        _db.Set<TEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Référentiel inséré : {Entity} ({Id})", typeof(TEntity).Name, id);
    }

    public async Task UpsertFromJsonAsync<TEntity>(Guid id, string payloadJson, CancellationToken ct = default)
        where TEntity : class
    {
        var entity = Deserialize<TEntity>(payloadJson);
        SetEntityId(entity, id);
        NormalizeEntity(entity);

        var set = _db.Set<TEntity>();
        var existing = await set.FindAsync([id], ct);
        if (existing is null)
        {
            set.Add(entity);
            _logger.LogInformation("Référentiel inséré (upsert) : {Entity} ({Id})", typeof(TEntity).Name, id);
        }
        else
        {
            _db.Entry(existing).CurrentValues.SetValues(entity);
            if (existing is BaseModel updated)
                updated.ModifierLe = DateTime.UtcNow;
            if (existing is Port existingPort)
                NormalizePort(existingPort);
            _logger.LogInformation("Référentiel mis à jour : {Entity} ({Id})", typeof(TEntity).Name, id);
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync<TEntity>(Guid id, CancellationToken ct = default)
        where TEntity : class
    {
        var entity = await _db.Set<TEntity>().FindAsync([id], ct);
        if (entity is null) return;

        _db.Set<TEntity>().Remove(entity);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Référentiel supprimé : {Entity} ({Id})", typeof(TEntity).Name, id);
    }

    public async Task InsertPaysAsync(Guid id, string code, string nom, bool actif, CancellationToken ct = default)
    {
        if (await _db.Pays.FindAsync([id], ct) is not null) return;

        _db.Pays.Add(new Pays
        {
            Id = id,
            Code = code,
            Nom = nom,
            Actif = actif,
            CreerLe = DateTime.UtcNow,
            ModifierLe = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Pays inséré : {Code} ({Id})", code, id);
    }

    public async Task UpsertPaysAsync(Guid id, string code, string nom, bool actif, CancellationToken ct = default)
    {
        var existing = await _db.Pays.FindAsync([id], ct);
        if (existing is null)
        {
            await InsertPaysAsync(id, code, nom, actif, ct);
            return;
        }

        existing.Code = code;
        existing.Nom = nom;
        existing.Actif = actif;
        existing.ModifierLe = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Pays mis à jour : {Code} ({Id})", code, id);
    }

    public async Task DeletePaysAsync(Guid id, CancellationToken ct = default) =>
        await DeleteAsync<Pays>(id, ct);

    public async Task UpsertPaysFromJsonAsync(Guid id, string payloadJson, CancellationToken ct = default)
    {
        var pays = Deserialize<Pays>(payloadJson);
        await UpsertPaysAsync(id, pays.Code, pays.Nom, pays.Actif, ct);
    }

    private static TEntity Deserialize<TEntity>(string payloadJson)
        where TEntity : class =>
        JsonSerializer.Deserialize<TEntity>(payloadJson, JsonOptions)
        ?? throw new InvalidOperationException($"Payload JSON invalide pour {typeof(TEntity).Name}.");

    private static void SetEntityId<TEntity>(TEntity entity, Guid id)
    {
        var idProperty = typeof(TEntity).GetProperty("Id");
        if (idProperty?.PropertyType == typeof(Guid) && idProperty.CanWrite)
            idProperty.SetValue(entity, id);
    }

    private static void NormalizeEntity<TEntity>(TEntity entity)
    {
        if (entity is BaseModel audit)
        {
            if (audit.CreerLe == default)
                audit.CreerLe = DateTime.UtcNow;
            audit.ModifierLe ??= DateTime.UtcNow;
        }

        if (entity is Port port)
            NormalizePort(port);
    }

    private static void NormalizePort(Port port)
    {
        if (string.IsNullOrWhiteSpace(port.Module) && !string.IsNullOrWhiteSpace(port.Type))
            port.Module = port.Type;
        if (string.IsNullOrWhiteSpace(port.Module))
            port.Module = port.Type ?? string.Empty;
    }
}
