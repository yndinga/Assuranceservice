namespace AssuranceService.Application.Common;

/// <summary>
/// Service pour générer les numéros de police et certificat
/// </summary>
public interface INumeroGeneratorService
{
    /// <summary>
    /// Genere le numero de police: {CodeAssureur}{Compteur}{AAMM}
    /// Exemple: AMC0000012607
    /// </summary>
    Task<string> GenerateNoPoliceLAsync(string codePartenaire);
    
    /// <summary>
    /// Génère le numéro de certificat: {Compteur}{AAMMJJ}
    /// Exemple: 100001251105
    /// </summary>
    Task<string> GenerateNumeroCertAsync();
    
    /// <summary>
    /// Obtient le prochain compteur disponible
    /// </summary>
    Task<int> GetNextCompteurAsync();
}



