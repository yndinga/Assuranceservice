using MediatR;

namespace AssuranceService.Application.Garanties.Commands;

public record UpdateGarantieCommand : IRequest<bool>
{
    public Guid Id { get; init; }
    public string NomGarantie { get; init; } = string.Empty;
    public string? Taux { get; init; }
    public decimal Accessoires { get; init; }
    public bool Actif { get; init; } = true;
}
