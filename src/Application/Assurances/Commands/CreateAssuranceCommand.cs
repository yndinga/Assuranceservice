using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

public record CreateAssuranceCommand : IRequest<Guid>
{
    // NoPolice et NumeroCert sont générés automatiquement lors de la soumission (Étape 4)
    public string ImportateurNom { get; init; } = string.Empty;
    public string? ImportateurNIU { get; init; }
    public DateTime? DateDebut { get; init; }
    public DateTime? DateFin { get; init; }
    public string TypeContrat { get; init; } = string.Empty;
    public string? Duree { get; init; }
    /// <summary>Code module (ex: MA, AE, RO, FL). Obligatoire.</summary>
    public string Module { get; init; } = string.Empty;
    public Guid? AssureurId { get; init; }
    public Guid? IntermediaireId { get; init; }
    public Guid? GarantieId { get; init; }
    public string? OCRE { get; init; }
    public string? Statut { get; init; }

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

    // Détails transport par module
    public Guid? PortEmbarquement { get; init; }
    public Guid? PortDebarquement { get; init; }
    public string? AeroportEmbarquement { get; init; }
    public string? AeroportDebarquement { get; init; }
    public string? RouteNationale { get; init; }
}



