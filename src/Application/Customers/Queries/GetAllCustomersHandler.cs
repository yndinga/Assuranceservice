using AssuranceService.Application.Common;
using MediatR;

namespace AssuranceService.Application.Customers.Queries;

public class GetAllCustomersHandler : IRequestHandler<GetAllCustomersQuery, IEnumerable<AssuranceService.Domain.Entities.Customer>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetAllCustomersHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IEnumerable<AssuranceService.Domain.Entities.Customer>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        return await _customerRepository.GetAllAsync();
    }
}





