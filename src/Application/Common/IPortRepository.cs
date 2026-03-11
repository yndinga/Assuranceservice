namespace AssuranceService.Application.Common;

/// <summary>Référentiel des ports (maritimes / fluviaux).</summary>
public interface IPortRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
