using AssuranceService.Application.Common;
using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AssuranceService.Infrastructure.ExternalServices;

/// <summary>
/// Service de taux de change en local : lit depuis la table TauxDeChanges ou la configuration.
/// Plus d'appel au microservice externe (localhost:5002).
/// </summary>
public class LocalTauxChangeService : ITauxChangeService
{
    private readonly AssuranceDbContext _context;
    private readonly IConfiguration _configuration;

    public LocalTauxChangeService(AssuranceDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<decimal> GetTauxChangeAsync(string devise, DateTime? date = null)
    {
        var code = (devise ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(code))
            throw new InvalidOperationException("Le code devise est requis pour obtenir le taux de change.");

        // 1. Taux depuis la table TauxDeChanges (référentiel local)
        var tauxRow = await _context.Devises
            .Where(d => d.Code == code)
            .Join(
                _context.TauxDeChanges.Where(t => t.Actif),
                d => d.Id,
                t => t.DeviseId,
                (_, t) => t)
            .OrderByDescending(t => t.ValideDe)
            .Select(t => (decimal?)t.Taux)
            .FirstOrDefaultAsync();

        if (tauxRow.HasValue)
            return tauxRow.Value;

        // 2. Sinon, taux depuis la configuration (section TauxChange:Local)
        var configKey = $"TauxChange:Local:{code}";
        var tauxFromConfig = _configuration[configKey];
        if (!string.IsNullOrWhiteSpace(tauxFromConfig) && decimal.TryParse(tauxFromConfig, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var configTaux))
            return configTaux;

        // 3. XOF / FCFA = 1 (par convention)
        if (code == "XOF" || code == "FCFA" || code == "XAF")
            return 1m;

        throw new InvalidOperationException($"Taux de change pour la devise {devise} introuvable. Renseignez la table TauxDeChanges ou la configuration TauxChange:Local:{code}.");
    }
}
