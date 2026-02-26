using AssuranceService.Application.Common;
using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Voyages.Queries;

public class GetVoyageByIdHandler : IRequestHandler<GetVoyageByIdQuery, VoyageDto?>
{
    private readonly IVoyageRepository _voyageRepository;

    public GetVoyageByIdHandler(IVoyageRepository voyageRepository)
    {
        _voyageRepository = voyageRepository;
    }

    public async Task<VoyageDto?> Handle(GetVoyageByIdQuery request, CancellationToken cancellationToken)
    {
        var voyage = await _voyageRepository.GetByIdAsync(request.Id);
        return voyage?.ToDto();
    }
}



