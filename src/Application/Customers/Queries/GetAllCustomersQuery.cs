using AssuranceService.Domain.Entities;
using MediatR;

namespace AssuranceService.Application.Customers.Queries;

public record GetAllCustomersQuery : IRequest<IEnumerable<Customer>>;





