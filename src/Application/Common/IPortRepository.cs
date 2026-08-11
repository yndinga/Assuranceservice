namespace AssuranceService.Application.Common;

/// <summary>Référentiel des ports (maritimes / fluviaux).</summary>
public interface IPortRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid?> GetIdByCodeAsync(string? code, string? type = null, CancellationToken cancellationToken = default);
}
