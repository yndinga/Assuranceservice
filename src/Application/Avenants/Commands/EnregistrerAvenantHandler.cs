using AssuranceService.Application.Avenants.DTOs;
using AssuranceService.Application.Common;
using MediatR;

namespace AssuranceService.Application.Avenants.Commands;

public class EnregistrerAvenantHandler : IRequestHandler<EnregistrerAvenantCommand, EnregistrerAvenantResponse>
{
    private readonly IAvenantRegistrationService _registrationService;

    public EnregistrerAvenantHandler(IAvenantRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public Task<EnregistrerAvenantResponse> Handle(EnregistrerAvenantCommand request, CancellationToken cancellationToken) =>
        _registrationService.EnregistrerAsync(request, cancellationToken);
}
