using AssuranceService.Application.Common;
using MediatR;

namespace AssuranceService.Application.Garanties.Commands;

public class DeleteGarantieHandler : IRequestHandler<DeleteGarantieCommand, bool>
{
    private readonly IGarantieRepository _garantieRepository;

    public DeleteGarantieHandler(IGarantieRepository garantieRepository)
    {
        _garantieRepository = garantieRepository;
    }

    public async Task<bool> Handle(DeleteGarantieCommand request, CancellationToken cancellationToken)
    {
        if (!await _garantieRepository.ExistsAsync(request.Id))
            return false;

        await _garantieRepository.DeleteAsync(request.Id);
        return true;
    }
}
