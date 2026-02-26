using AssuranceService.Application.Common;

namespace AssuranceService.Application.Services;

/// <summary>
/// Service de génération des numéros de police et certificat
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
    /// Génère le NoPolice: {CodePartenaire}{Compteur}{JJMMAA}
    /// Exemple: SUN100001051125
    /// </summary>
    public async Task<string> GenerateNoPoliceLAsync(string codePartenaire)
    {
        var compteur = await GetNextCompteurAsync();
        var date = DateTime.Now;
        var dateFormat = date.ToString("ddMMyy"); // JJMMAA
        
        return $"{codePartenaire}{compteur:D6}{dateFormat}";
    }

    /// <summary>
    /// Génère le NumeroCert selon la formule: {Compteur:D6}{AAMMJJ}
    /// Exemple: 000120260719  (compteur=000120, année=26, mois=07, jour=19)
    /// L'année (2 chiffres) garantit qu'aucun doublon ne peut survenir d'une année à l'autre,
    /// même si la BD était réinitialisée. La contrainte UNIQUE en base reste le filet de sécurité final.
    /// </summary>
    public async Task<string> GenerateNumeroCertAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var jour = DateTime.Now.ToString("yyMMdd"); // AAMMJJ — ex: "260219" pour le 19 février 2026

            var lastNumeroCert = await _assuranceRepository.GetLastNumeroCertAsync();

            int prochainCompteur;
            if (string.IsNullOrEmpty(lastNumeroCert))
            {
                prochainCompteur = 1; // premier enregistrement → "000001"
            }
            else
            {
                // Extraire les 6 premiers chiffres (séquence) et incrémenter
                prochainCompteur = int.Parse(lastNumeroCert.Substring(0, 6)) + 1;
            }

            return prochainCompteur.ToString("D6") + jour;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Obtient le prochain compteur disponible pour le NoPolice (thread-safe).
    /// </summary>
    public async Task<int> GetNextCompteurAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return await GetDernierCompteurNoPolicAsync() + 1;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<int> GetDernierCompteurNoPolicAsync()
    {
        var assurances = await _assuranceRepository.GetAllAsync();

        var dernierCompteur = assurances
            .Where(a => !string.IsNullOrEmpty(a.NoPolice) && a.NoPolice.Length >= 9)
            .Select(a =>
            {
                // NoPolice format: {CodePartenaire(3)}{Compteur(6)}{JJMMAA(6)}
                if (int.TryParse(a.NoPolice!.Substring(3, 6), out var c)) return (int?)c;
                return null;
            })
            .Where(c => c.HasValue)
            .OrderByDescending(c => c!.Value)
            .FirstOrDefault();

        return dernierCompteur ?? 100000; // Démarre à 100001
    }
}

