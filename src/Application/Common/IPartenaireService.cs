namespace AssuranceService.Application.Common;

/// <summary>
/// Service pour obtenir les informations des partenaires depuis le microservice externe
/// </summary>
public interface IPartenaireService
{
    /// <summary>
    /// Obtient le code du partenaire (ex: "SUN") depuis le MS Partenaires
    /// </summary>
    Task<string> GetCodePartenaireAsync(Guid partenaireId);
    
    /// <summary>
    /// Obtient les informations complètes du partenaire
    /// </summary>
    Task<PartenaireDto?> GetPartenaireAsync(Guid partenaireId);
}

public record PartenaireDto
{
    public Guid Id { get; init; }
    public string Nom { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}



