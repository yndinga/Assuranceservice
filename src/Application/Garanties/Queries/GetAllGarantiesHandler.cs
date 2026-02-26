using AssuranceService.Application.Common;
using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Garanties.Queries;

public class GetAllGarantiesHandler : IRequestHandler<GetAllGarantiesQuery, IEnumerable<GarantieDto>>
{
    private readonly IGarantieRepository _garantieRepository;

    public GetAllGarantiesHandler(IGarantieRepository garantieRepository)
    {
        _garantieRepository = garantieRepository;
    }

    public async Task<IEnumerable<GarantieDto>> Handle(GetAllGarantiesQuery request, CancellationToken cancellationToken)
    {
        var garanties = await _garantieRepository.GetAllAsync();
        return garanties.ToDtoList();
    }
}



