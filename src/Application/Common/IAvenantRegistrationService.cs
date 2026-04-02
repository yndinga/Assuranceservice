using AssuranceService.Application.Avenants.Commands;
using AssuranceService.Application.Avenants.DTOs;

namespace AssuranceService.Application.Common;

public interface IAvenantRegistrationService
{
    Task<EnregistrerAvenantResponse> EnregistrerAsync(EnregistrerAvenantCommand command, CancellationToken cancellationToken = default);
}
