using MediatR;

namespace AssuranceService.Application.Voyages.Commands;

public record CreateVoyageCommand : IRequest<Guid>
{
    public Guid AssuranceId { get; init; }

    public Guid ModuleId { get; init; }

    // Informations transporteur
    public string NomTransporteur { get; init; } = string.Empty;
    public string NomNavire { get; init; } = string.Empty;
    public string TypeNavire { get; init; } = string.Empty;
    
    // Lieux communs
    public string PaysProvenance { get; init; } = string.Empty;
    public string PaysDestination { get; init; } = string.Empty;

    // Séjour (informationnel, non stocké en base)
    public string? LieuSejour { get; init; }
    public string? DureeSejour { get; init; }

    // 🚢 Transport Maritime / 🌊 Fluvial — même table Ports (référentiel)
    public Guid? PortEmbarquementId { get; init; }
    public Guid? PortDebarquementId { get; init; }
    
    // ✈️ Transport Aérien
    public string? AeroportEmbarquement { get; init; }
    public string? AeroportDebarquement { get; init; }
    
    // 🚛 Transport Terrestre
    public string? RouteNationale { get; init; }
    // CreerPar et ModifierPar gérés automatiquement
}

