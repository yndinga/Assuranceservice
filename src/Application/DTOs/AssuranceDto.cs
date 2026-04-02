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
    public string Statut { get; init; } = "10";
    public string Module { get; init; } = string.Empty;

    public Guid? GarantieId { get; init; }
    public string? GarantieNom { get; init; }
    
    public Guid? AssureurId { get; init; }
    public Guid? IntermediaireId { get; init; }
    public string OCRE { get; init; } = string.Empty;

    // Données cargaison (fusionnées)
    public string? Designation { get; init; }
    public string? Nature { get; init; }
    public string? Specificites { get; init; }
    public string? Conditionnement { get; init; }
    public string? Description { get; init; }
    public decimal? ValeurFCFA { get; init; }
    public decimal? ValeurDevise { get; init; }
    public string? Devise { get; init; }
    public string? MasseBrute { get; init; }
    public string? UniteStatistique { get; init; }
    public string? Marque { get; init; }

    // Transport (fusionné)
    public string? NomTransporteur { get; init; }
    public string? NomNavire { get; init; }
    public string? TypeNavire { get; init; }
    public string? LieuSejour { get; init; }
    public string? DureeSejour { get; init; }
    public string? PaysProvenance { get; init; }
    public string? PaysDestination { get; init; }

    // Détails transport par module (issus des tables filles)
    public Guid? PortEmbarquement { get; init; }
    public Guid? PortDebarquement { get; init; }
    public string? AeroportEmbarquement { get; init; }
    public string? AeroportDebarquement { get; init; }
    public string? RouteNationale { get; init; }

    // Audit
    public string CreerPar { get; init; } = string.Empty;
    public string ModifierPar { get; init; } = string.Empty;
    public DateTime CreerLe { get; init; }
    public DateTime? ModifierLe { get; init; }
}
