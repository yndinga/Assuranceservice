using AssuranceService.Application.Common;

namespace AssuranceService.Application.Services;

/// <summary>
/// Service de generation des numeros de police et certificat.
/// </summary>
public class NumeroGeneratorService : INumeroGeneratorService
{
    private readonly IAssuranceRepository _assuranceRepository;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public NumeroGeneratorService(IAssuranceRepository assuranceRepository)
    {
        _assuranceRepository = assuranceRepository;
    }

    /// <summary>
    /// Genere le NoPolice: {CodeAssureur}{Compteur}{AAMM}. Exemple: AMC0000012607.
    /// </summary>
    public async Task<string> GenerateNoPoliceLAsync(string codePartenaire)
    {
        var codeAssureur = NormalizeAssureurCode(codePartenaire);
        var compteur = await GetNextCompteurAsync();
        var dateFormat = DateTime.Now.ToString("yyMM");

        return $"{codeAssureur}{compteur:D6}{dateFormat}";
    }

    /// <summary>
    /// Genere le NumeroCert sans dependance a l'assureur: {Compteur:D6}{AAMMJJ}.
    /// </summary>
    public async Task<string> GenerateNumeroCertAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var jour = DateTime.Now.ToString("yyMMdd");
            var lastNumeroCert = await _assuranceRepository.GetLastNumeroCertAsync();

            int prochainCompteur;
            if (string.IsNullOrEmpty(lastNumeroCert))
            {
                prochainCompteur = 1;
            }
            else
            {
                prochainCompteur = int.Parse(lastNumeroCert[..6]) + 1;
            }

            return prochainCompteur.ToString("D6") + jour;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<int> GetNextCompteurAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return await GetDernierCompteurNoPoliceAsync() + 1;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<int> GetDernierCompteurNoPoliceAsync()
    {
        var assurances = await _assuranceRepository.GetAllAsync();

        var dernierCompteur = assurances
            .Where(a => !string.IsNullOrWhiteSpace(a.NoPolice) && a.NoPolice.Length >= 13)
            .Select(a =>
            {
                if (int.TryParse(a.NoPolice![3..9], out var compteur))
                {
                    return (int?)compteur;
                }

                return null;
            })
            .Where(compteur => compteur.HasValue)
            .OrderByDescending(compteur => compteur!.Value)
            .FirstOrDefault();

        return dernierCompteur ?? 0;
    }

    private static string NormalizeAssureurCode(string codePartenaire)
    {
        var normalized = new string((codePartenaire ?? string.Empty)
            .Trim()
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());

        if (normalized.Length < 3)
        {
            throw new InvalidOperationException("Le code assureur doit contenir au moins 3 caracteres.");
        }

        return normalized[..3];
    }
}
