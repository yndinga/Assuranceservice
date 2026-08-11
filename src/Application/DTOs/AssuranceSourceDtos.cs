namespace AssuranceService.Application.DTOs;

public sealed record AssuranceDeclarationDto
{
    public Guid Id { get; init; }
    public Guid DeclarationId { get; init; }
    public string? NumeroDI { get; init; }
    public string PaysEmbarquementCode { get; init; } = string.Empty;
    public string PortEmbarquementCode { get; init; } = string.Empty;
}

public sealed record AssuranceLigneDto
{
    public Guid Id { get; init; }
    public Guid DeclarationId { get; init; }
    public Guid SourceCommandeId { get; init; }
    public Guid SourceLigneDIId { get; init; }
    public int NoLigne { get; init; }
    public string PositionTarifaire { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    public string Marque { get; init; } = string.Empty;
    public string Colisage { get; init; } = string.Empty;
    public decimal? Quantite { get; init; }
    public decimal? MasseBrute { get; init; }
    public decimal? MasseNette { get; init; }
    public decimal? Volume { get; init; }
    public decimal? PrixUnitaire { get; init; }
    public decimal? ValeurDevise { get; init; }
    public decimal? ValeurXAF { get; init; }
    public string? Devise { get; init; }
    public string? UniteStatistique { get; init; }
    public string? PaysOrigine { get; init; }
    public bool EstPartielle { get; init; }
}
