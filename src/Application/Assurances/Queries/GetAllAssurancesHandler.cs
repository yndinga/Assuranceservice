using AssuranceService.Application.Common;
using AssuranceService.Application.DTOs;
using MediatR;

namespace AssuranceService.Application.Assurances.Queries;

public class GetAllAssurancesHandler : IRequestHandler<GetAllAssurancesQuery, PagedResult<AssuranceDto>>
{
    private readonly IAssuranceRepository _assuranceRepository;

    public GetAllAssurancesHandler(IAssuranceRepository assuranceRepository)
    {
        _assuranceRepository = assuranceRepository;
    }

    public async Task<PagedResult<AssuranceDto>> Handle(GetAllAssurancesQuery request, CancellationToken cancellationToken)
    {
        const int page = 1;
        const int perPage = 10;

        var (items, totalCount) = await _assuranceRepository.GetPagedAsync(
            request.Search,
            page,
            perPage,
            request.Ocre,
            request.IntermediaireId,
            request.AssureurId);

        var data = items.ToDtoList().ToList();
        return new PagedResult<AssuranceDto>
        {
            Data = data,
            Total = totalCount,
            Page = page,
            PerPage = perPage
        };
    }
}



