using AssuranceService.Application.Policies.Commands;
using FluentValidation;

namespace AssuranceService.Application.Policies;

public class CreatePolicyValidator : AbstractValidator<CreatePolicyCommand>
{
    public CreatePolicyValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Premium).GreaterThan(0);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}
