using AssuranceService.Application.Common;
using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Assurances.Queries;

public class GetAssuranceByIdHandler : IRequestHandler<GetAssuranceByIdQuery, AssuranceDetailDto?>
{
    private readonly IAssuranceRepository _assuranceRepository;

    public GetAssuranceByIdHandler(IAssuranceRepository assuranceRepository)
    {
        _assuranceRepository = assuranceRepository;
    }

    public async Task<AssuranceDetailDto?> Handle(GetAssuranceByIdQuery request, CancellationToken cancellationToken)
    {
        var assurance = await _assuranceRepository.GetByIdAsync(request.Id);
        return assurance?.ToDetailDto();
    }
}



