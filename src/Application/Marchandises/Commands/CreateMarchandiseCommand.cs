using MediatR;

namespace AssuranceService.Application.Marchandises.Commands;

public record CreateMarchandiseCommand : IRequest<Guid>
{
    public string Designation { get; init; } = string.Empty;
    public string? Nature { get; init; }
    public string? Specificites { get; init; }
    public string Conditionnement { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid AssuranceId { get; init; }
    public decimal Valeur { get; init; }
    public decimal? ValeurDevise { get; init; }
    public string Devise { get; init; } = string.Empty;
    public string? MasseBrute { get; init; }
    public string? UniteStatistique { get; init; }
    public string? Marque { get; init; }
    // CreerPar et ModifierPar gérés automatiquement
}



