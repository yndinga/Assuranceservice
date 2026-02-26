using MediatR;

namespace AssuranceService.Application.Garanties.Commands;

public record CreateGarantieCommand : IRequest<Guid>
{
    public string NomGarantie { get; init; } = string.Empty;
    public string? Taux { get; init; }
    public decimal Accessoires { get; init; }
    public bool Actif { get; init; } = true;
    // CreerPar et ModifierPar gérés automatiquement
}



