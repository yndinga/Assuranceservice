using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

public record UpdateAssuranceCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public string? NoPolice { get; init; }
    public string? NumeroCert { get; init; }
    public string Importateur { get; init; } = string.Empty;
    public DateTime? DateDebut { get; init; }
    public DateTime? DateFin { get; init; }
    public string TypeContrat { get; init; } = string.Empty;
    public string? Duree { get; init; }
    public string Statut { get; init; } = "42";
    public string ModeDeTransport { get; init; } = string.Empty;
    public string? Assureur { get; init; }
    public string? Intermediaire { get; init; }
    public string? TypePartenaire { get; init; }
    public string? Garantie { get; init; }
    public string NomTransporteur { get; init; } = string.Empty;
    public string? NomNavire { get; init; }
    public string? TypeNavire { get; init; }
    public string ModifierPar { get; init; } = string.Empty;
}





