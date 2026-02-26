using MediatR;

namespace AssuranceService.Application.Policies.Commands;

public record CreatePolicyCommand(string Number, Guid CustomerId, decimal Premium, DateTime StartDate, DateTime EndDate) : IRequest<Guid>;
