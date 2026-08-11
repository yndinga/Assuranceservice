using MediatR;

namespace AssuranceService.Application.Assurances.Commands;

public record CreateAssuranceCommand : IRequest<Guid>
{
    // NoPolice et NumeroCert sont générés automatiquement lors de la signature.
    public string ImportateurNom { get; init; } = string.Empty;
    public string? ImportateurNIU { get; init; }
    public DateTime? DateDebut { get; init; }
    public DateTime? DateFin { get; init; }
    public string TypeContrat { get; init; } = string.Empty;
    public string? Duree { get; init; }
    /// <summary>Code mode de transport (ex: MA, AR, RO, FL). Obligatoire.</summary>
    public string ModeDeTransport { get; init; } = string.Empty;
    public string? Assureur { get; init; }
    public string? Intermediaire { get; init; }
    public string? TypePartenaire { get; init; }
    public string? Garantie { get; init; }
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

    // Transport (fusionné) — NomNavire/TypeNavire : MA et FL uniquement
    public string? NomTransporteur { get; init; }
    public string? NomNavire { get; init; }
    public string? TypeNavire { get; init; }
    public string? Transit { get; init; }
    public string? PaysProvenance { get; init; }
    public string? PaysDestination { get; init; }

    // Détails transport par module
    public string? PortEmbarquement { get; init; }
    public string? PortDebarquement { get; init; }
    public string? AeroportEmbarquement { get; init; }
    public string? AeroportDebarquement { get; init; }
    public string? RouteNationale { get; init; }
    /// <summary>Numéro de connaissement (BL) — mode maritime (MA) uniquement.</summary>
    public string? NumeroBL { get; init; }
    /// <summary>Numéro LTA (lettre de transport aérien) — mode aérien (AR) uniquement.</summary>
    public string? NumeroLTA { get; init; }
    /// <summary>Numéro de lettre de voiture — mode routier (RO) uniquement.</summary>
    public string? NumeroLV { get; init; }
}
