using AssuranceService.Domain.Models;

namespace AssuranceService.Application.Common;

public interface IDocumentRepository
{
    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetByAssuranceIdAsync(Guid assuranceId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
