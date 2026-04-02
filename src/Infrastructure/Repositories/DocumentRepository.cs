using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssuranceService.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AssuranceDbContext _db;

    public DocumentRepository(AssuranceDbContext db) => _db = db;

    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        _db.Documents.Add(document);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return document;
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _db.Documents.FirstOrDefaultAsync(d => d.Id == id, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Document>> GetByAssuranceIdAsync(Guid assuranceId, CancellationToken cancellationToken = default)
        => await _db.Documents
            .Where(d => d.AssuranceId == assuranceId)
            .OrderByDescending(d => d.CreerLe)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<bool> ExistsByAssuranceIdAsync(Guid assuranceId, CancellationToken cancellationToken = default)
        => await _db.Documents
            .AnyAsync(d => d.AssuranceId == assuranceId, cancellationToken)
            .ConfigureAwait(false);

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == id, cancellationToken).ConfigureAwait(false);
        if (doc == null) return false;
        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
