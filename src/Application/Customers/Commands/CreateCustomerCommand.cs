using MediatR;

namespace AssuranceService.Application.Customers.Commands;

public record CreateCustomerCommand : IRequest<Guid>
{
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
}





