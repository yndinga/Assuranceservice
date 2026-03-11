namespace AssuranceService.Application.DTOs;

public record VoyageDto
{
    public Guid ID { get; init; }
    public Guid AssuranceId { get; init; }

    /// <summary>Code du type de transport (code uniquement, pas d'ID) : MA, AE, RO, FL.</summary>
    public string ModuleCode { get; init; } = string.Empty;
    public string NomTransporteur { get; init; } = string.Empty;
    public string NomNavire { get; init; } = string.Empty;
    public string TypeNavire { get; init; } = string.Empty;
    /// <summary>Défini par l'importateur, peut être null.</summary>
    public string? LieuSejour { get; init; }
    /// <summary>Défini par l'importateur, peut être null.</summary>
    public string? DureeSejour { get; init; }
    public string PaysProvenance { get; init; } = string.Empty;
    public string PaysDestination { get; init; } = string.Empty;

    // Un seul sera rempli selon TypeTransport
    public MaritimeDto? Maritime { get; init; }
    public AerienDto? Aerien { get; init; }
    public RoutierDto? Routier { get; init; }
    public FluvialDto? Fluvial { get; init; }

    public DateTime CreerLe { get; init; }
    public DateTime? ModifierLe { get; init; }
}

public record MaritimeDto
{
    public Guid PortEmbarquementId { get; init; }
    public string? PortEmbarquementNom { get; init; }
    public Guid PortDebarquementId { get; init; }
    public string? PortDebarquementNom { get; init; }
}

public record AerienDto
{
    public string AeroportEmbarquement { get; init; } = string.Empty;
    public string? AeroportDebarquement { get; init; }
}

public record RoutierDto
{
    public string RouteNationale { get; init; } = string.Empty;
}

public record FluvialDto
{
    public Guid PortEmbarquementId { get; init; }
    public string? PortEmbarquementNom { get; init; }
    public Guid? PortDebarquementId { get; init; }
    public string? PortDebarquementNom { get; init; }
}
