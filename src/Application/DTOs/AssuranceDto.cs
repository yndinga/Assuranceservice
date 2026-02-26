namespace AssuranceService.Application.DTOs;

/// <summary>
/// DTO pour Assurance (sans les collections de navigation)
/// </summary>
public record AssuranceDto
{
    public Guid ID { get; init; }
    
    // Numérotation
    public string? NoPolice { get; init; }
    public string? NumeroCert { get; init; }
    
    // Importateur
    public string ImportateurNom { get; init; } = string.Empty;
    public string? ImportateurNIU { get; init; }
    
    // Dates
    public DateTime? DateDebut { get; init; }
    public DateTime? DateFin { get; init; }
    
    // Contrat
    public string TypeContrat { get; init; } = string.Empty;
    public string? Duree { get; init; }
    public string Statut { get; init; } = string.Empty;
    
    public Guid? GarantieId { get; init; }
    public string? GarantieNom { get; init; }
    
    public Guid? AssureurId { get; init; }
    public Guid? IntermediaireId { get; init; }
    public string OCRE { get; init; } = string.Empty;

    // Audit
    public string CreerPar { get; init; } = string.Empty;
    public string ModifierPar { get; init; } = string.Empty;
    public DateTime CreerLe { get; init; }
    public DateTime? ModifierLe { get; init; }
}
