using AssuranceService.Domain.Models;

namespace AssuranceService.Application.Common;

public interface IAssuranceRepository
{
    Task<Assurance?> GetByIdAsync(Guid id);
    /// <summary>Récupère l'assurance avec Garantie uniquement (sans collections liées).</summary>
    Task<Assurance?> GetByIdMinimalAsync(Guid id);
    /// <summary>Récupère uniquement le code statut (ex: "10", "11").</summary>
    Task<string?> GetStatutAsync(Guid id);
    Task<IEnumerable<Assurance>> GetAllAsync();
    /// <summary>
    /// Liste paginée avec recherche (NoPolice, NumeroCert, ImportateurNom) et filtre par organisation connectée.
    /// Le filtre est déterminé par <paramref name="organisationType"/> : ASSUREUR → Assureur, COURTIER/AGENT_GENERAL → Intermediaire,
    /// sinon (IMPORTATEUR/TRANSITAIRE) → OCRE. Aucun code = administrateur (pas de filtre).
    /// </summary>
    Task<(IEnumerable<Assurance> Items, int TotalCount)> GetPagedAsync(string? search, int page, int perPage, string? organisationCode, string? organisationType);
    /// <summary>Assurances où l'assureur (maison d'assurance) est désigné — il peut les signer.</summary>
    Task<IEnumerable<Assurance>> GetByAssureurCodeAsync(string assureurCode);
    /// <summary>Assurances envoyées à l'intermédiaire (courtier / agent général) — il peut les voir et signer.</summary>
    Task<IEnumerable<Assurance>> GetByIntermediaireCodeAsync(string intermediaireCode);
    Task<Assurance> CreateAsync(Assurance assurance);
    Task<Assurance> UpdateAsync(Assurance assurance);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Assurance?> GetByNoPoliceAsync(string noPolice);
    /// <summary>Retourne le NumeroCert le plus récent (trié DESC) pour la génération séquentielle.</summary>
    Task<string?> GetLastNumeroCertAsync();
    Task<IReadOnlyDictionary<Guid, AssuranceLineConsumption>> GetLineConsumptionsAsync(
        IReadOnlyCollection<Guid> sourceLineIds,
        Guid? excludeAssuranceId = null,
        CancellationToken cancellationToken = default);
    
    Task AddVisaAssuranceAsync(VisaAssurance visaAssurance);
    Task SaveChangesAsync();
}

public sealed record AssuranceLineConsumption(
    decimal Quantite,
    decimal MasseBrute,
    decimal MasseNette,
    decimal Volume,
    decimal ValeurDevise,
    decimal ValeurXAF);
