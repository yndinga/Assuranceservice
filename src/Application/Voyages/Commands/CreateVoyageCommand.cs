using System.Text.Json.Serialization;
using MediatR;

namespace AssuranceService.Application.Voyages.Commands;

public record CreateVoyageCommand : IRequest<Guid>
{
    public Guid AssuranceId { get; init; }

    /// <summary>Code du type de transport (code uniquement, pas d'ID) : MA, AE, RO, FL.</summary>
    [JsonPropertyName("moduleCode")]
    public string ModuleCode { get; init; } = string.Empty;

    // Informations transporteur
    public string NomTransporteur { get; init; } = string.Empty;
    public string NomNavire { get; init; } = string.Empty;
    public string TypeNavire { get; init; } = string.Empty;
    
    // Lieux communs
    public string PaysProvenance { get; init; } = string.Empty;
    public string PaysDestination { get; init; } = string.Empty;

    // Séjour : défini par l'importateur, peut être null
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

