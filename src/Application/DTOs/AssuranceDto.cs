namespace AssuranceService.Application.DTOs;

/// <summary>
/// DTO pour Assurance (sans les collections de navigation)
/// </summary>
public record AssuranceDto
{
    public Guid ID { get; init; }
    public string NumeroAFI { get; init; } = string.Empty;
    
    // NumÃ©rotation
    public string? NoPolice { get; init; }
    public string? NumeroCert { get; init; }

    public string? NoFacture { get; init; }
    
    // Importateur
    public string ImportateurNom { get; init; } = string.Empty;
    public string? ImportateurNIU { get; init; }
    
    // Dates
    public DateTime? DateDebut { get; init; }
    public DateTime? DateFin { get; init; }
    
    // Contrat
    public string TypeContrat { get; init; } = string.Empty;
    public string? Duree { get; init; }
    public int? DureeJours { get; init; }
    public string Statut { get; init; } = "42";
    public string ModeDeTransport { get; init; } = string.Empty;

    public Guid? GarantieId { get; init; }
    public string? AssureurId { get; init; }
    public string? IntermediaireId { get; init; }
    public string Garantie { get; init; } = string.Empty;
    
    public string Assureur { get; init; } = string.Empty;
    public string Intermediaire { get; init; } = string.Empty;
    public string TypePartenaire { get; init; } = string.Empty;
    public string OCRE { get; init; } = string.Empty;

    // DonnÃ©es cargaison (fusionnÃ©es)
    public string? Designation { get; init; }
    public string? Nature { get; init; }
    public string? Specificites { get; init; }
    public string? Conditionnement { get; init; }
    public string? Description { get; init; }
    public decimal? ValeurFCFA { get; init; }
    public decimal? ValeurDevise { get; init; }
    public decimal ValeurXAFTotale { get; init; }
    public decimal ValeurDeviseTotale { get; init; }
    public decimal MasseBruteTotale { get; init; }
    public decimal MasseNetteTotale { get; init; }
    public decimal VolumeTotal { get; init; }
    public string? Devise { get; init; }
    public string? MasseBrute { get; init; }
    public string? UniteStatistique { get; init; }
    public string? Marque { get; init; }

    // Transport (fusionnÃ©)
    public string? NomTransporteur { get; init; }
    public string? NomNavire { get; init; }
    public string? TypeNavire { get; init; }
    public string? LieuSejour { get; init; }
    public string? DureeSejour { get; init; }
    public string? PaysProvenance { get; init; }
    public string? PaysDestination { get; init; }
    public string? PaysEmbarquementCode { get; init; }
    public string? PortEmbarquementCommunCode { get; init; }

    // DÃ©tails transport par module (issus des tables filles)
    public Guid? PortEmbarquement { get; init; }
    public Guid? PortDebarquement { get; init; }
    public string? PortEmbarquementCode { get; init; }
    public string? PortDebarquementCode { get; init; }
    public string? AeroportEmbarquement { get; init; }
    public string? AeroportDebarquement { get; init; }
    public string? RouteNationale { get; init; }
    public string? NumeroBL { get; init; }
    public string? NumeroLTA { get; init; }
    public string? NumeroLV { get; init; }

    // Audit
    public string CreerPar { get; init; } = string.Empty;
    public string ModifierPar { get; init; } = string.Empty;
    public DateTime CreerLe { get; init; }
    public DateTime? ModifierLe { get; init; }
}
