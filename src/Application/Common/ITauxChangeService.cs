namespace AssuranceService.Application.Common;

/// <summary>
/// Service pour obtenir les taux de change depuis le microservice externe
/// </summary>
public interface ITauxChangeService
{
    /// <summary>
    /// Obtient le taux de change d'une devise vers FCFA
    /// </summary>
    /// <param name="devise">Code de la devise (USD, EUR, etc.)</param>
    /// <param name="date">Date pour laquelle obtenir le taux (optionnel, par défaut aujourd'hui)</param>
    Task<decimal> GetTauxChangeAsync(string devise, DateTime? date = null);
}

public record TauxChangeDto
{
    public string Devise { get; init; } = string.Empty;
    public decimal Taux { get; init; }
    public DateTime Date { get; init; }
}



