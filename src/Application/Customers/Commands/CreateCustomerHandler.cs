using AssuranceService.Application.Common;
using AssuranceService.Domain.Entities;
using MediatR;

namespace AssuranceService.Application.Customers.Commands;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;

    public CreateCustomerHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer(request.FullName, request.Email);
        var createdCustomer = await _customerRepository.CreateAsync(customer);
        return createdCustomer.Id;
    }
}





