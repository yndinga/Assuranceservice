using FluentValidation;

namespace AssuranceService.Application.Marchandises.Commands;

public class CreateMarchandiseValidator : AbstractValidator<CreateMarchandiseCommand>
{
    public CreateMarchandiseValidator()
    {
        RuleFor(x => x.Designation)
            .NotEmpty().WithMessage("La désignation est requise.")
            .MaximumLength(255);

        RuleFor(x => x.Conditionnement)
            .NotEmpty().WithMessage("Le conditionnement est requis.")
            .MaximumLength(500);

        RuleFor(x => x.AssuranceId)
            .NotEmpty().WithMessage("L'identifiant de l'assurance est requis.");

        RuleFor(x => x.Valeur)
            .GreaterThanOrEqualTo(0).WithMessage("La valeur doit être positive ou nulle.");

        RuleFor(x => x.Nature)
            .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Nature));

        RuleFor(x => x.Specificites)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Specificites));

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Devise)
            .MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Devise));

        RuleFor(x => x.MasseBrute)
            .MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.MasseBrute));

        RuleFor(x => x.UniteStatistique)
            .MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.UniteStatistique));

        RuleFor(x => x.Marque)
            .MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.Marque));
    }
}
