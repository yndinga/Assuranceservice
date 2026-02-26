using AssuranceService.Application.Common;
using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Garanties.Queries;

public class GetGarantieByIdHandler : IRequestHandler<GetGarantieByIdQuery, GarantieDto?>
{
    private readonly IGarantieRepository _garantieRepository;

    public GetGarantieByIdHandler(IGarantieRepository garantieRepository)
    {
        _garantieRepository = garantieRepository;
    }

    public async Task<GarantieDto?> Handle(GetGarantieByIdQuery request, CancellationToken cancellationToken)
    {
        var garantie = await _garantieRepository.GetByIdAsync(request.Id);
        return garantie?.ToDto();
    }
}
