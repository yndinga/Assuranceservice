using AssuranceService.Application.Common;
using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

public class DeleteAssuranceHandler : IRequestHandler<DeleteAssuranceCommand, Unit>
{
    private readonly IAssuranceRepository _assuranceRepository;

    public DeleteAssuranceHandler(IAssuranceRepository assuranceRepository)
    {
        _assuranceRepository = assuranceRepository;
    }

    public async Task<Unit> Handle(DeleteAssuranceCommand request, CancellationToken cancellationToken)
    {
        var assurance = await _assuranceRepository.GetByIdAsync(request.Id);
        if (assurance == null)
        {
            throw new ArgumentException($"Assurance with ID {request.Id} not found.");
        }

        await _assuranceRepository.DeleteAsync(request.Id);
        return Unit.Value;
    }
}





