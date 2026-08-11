namespace AssuranceService.Application.Common;

public interface IDeclarationDossierClient
{
    Task<DeclarationDossierSource?> GetDossierAsync(Guid dossierId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DeclarationDossierSource>> GetDossiersAsync(CancellationToken cancellationToken = default);
}

public sealed record DeclarationDossierSource
{
    public Guid Id { get; init; }
    public string? NoDossier { get; init; }
    public int Etat { get; init; }
    public string? Regime { get; init; }
    public string? OCRE { get; init; }
    public string? PCRE { get; init; }
    public string? Transitaire { get; init; }
    public string? ImportateurNom { get; init; }
    public string? ImportateurNIU { get; init; }
    public string? ModeDeTransport { get; init; }
    public IReadOnlyCollection<DeclarationCommandeSource> Commandes { get; init; } = Array.Empty<DeclarationCommandeSource>();

    public Guid EffectiveId => Id;
}

public sealed record DeclarationCommandeSource
{
    public Guid Id { get; init; }
    public Guid DossierId { get; init; }
    public string? NoFacture { get; init; }
    public string? Intitule { get; init; }
    public string? Devise { get; init; }
    public string? PaysProvenance { get; init; }
    public string? PaysEmbarquement { get; init; }
    public string? PaysDestination { get; init; }
    public string? PaysDebarquement { get; init; }
    public decimal? ValeurDevise { get; init; }
    public decimal? ValeurFCFA { get; init; }
    public decimal? AssuranceDevise { get; init; }
    public decimal? AssuranceFCFA { get; init; }
    public decimal? MasseBrute { get; init; }
    public decimal? MasseNette { get; init; }
    public decimal? Volume { get; init; }
    public string? PortEmbarquement { get; init; }
    public string? PortDebarquement { get; init; }
    public string? AeroportEmbarquement { get; init; }
    public string? AeroportDebarquement { get; init; }
    public string? RouteNationale { get; init; }
    public string? FleuveEmbarquement { get; init; }
    public string? FleuveDebarquement { get; init; }
    public IReadOnlyCollection<DeclarationLigneCommandeSource> LignesCommandes { get; init; } = Array.Empty<DeclarationLigneCommandeSource>();

    public Guid EffectiveId => Id;
}

public sealed record DeclarationLigneCommandeSource
{
    public Guid Id { get; init; }
    public Guid CommandeId { get; init; }
    public int NoLigne { get; init; }
    public string? PositionTarifaire { get; init; }
    public string? Designation { get; init; }
    public string? Marque { get; init; }
    public string? Colisage { get; init; }
    public decimal? MasseBrute { get; init; }
    public decimal? MasseNette { get; init; }
    public decimal? Volume { get; init; }
    public decimal? Quantite { get; init; }
    public decimal? PrixUnitaire { get; init; }
    public decimal? ValeurDevise { get; init; }
    public decimal? ValeurFCFA { get; init; }
    public string? UniteStatistique { get; init; }
    public string? PaysOrigine { get; init; }
}
