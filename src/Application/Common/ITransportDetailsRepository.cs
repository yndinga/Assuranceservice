using AssuranceService.Domain.Models;

namespace AssuranceService.Application.Common;

public interface ITransportDetailsRepository
{
    Task AddMaritimeAsync(Maritime maritime, CancellationToken cancellationToken = default);
    Task AddAerienAsync(Aerien aerien, CancellationToken cancellationToken = default);
    Task AddRoutierAsync(Routier routier, CancellationToken cancellationToken = default);
    Task AddFluvialAsync(Fluvial fluvial, CancellationToken cancellationToken = default);
}
