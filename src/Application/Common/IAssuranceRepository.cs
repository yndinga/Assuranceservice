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
    /// <summary>Liste paginée avec recherche (NoPolice, NumeroCert, ImportateurNom) et filtres rôle (OCRE, IntermediaireId, AssureurId).</summary>
    Task<(IEnumerable<Assurance> Items, int TotalCount)> GetPagedAsync(string? search, int page, int perPage, string? ocre, Guid? intermediaireId, Guid? assureurId);
    /// <summary>Assurances où l'assureur (maison d'assurance) est désigné — il peut les signer.</summary>
    Task<IEnumerable<Assurance>> GetByAssureurIdAsync(Guid assureurId);
    /// <summary>Assurances envoyées à l'intermédiaire (courtier / agent général) — il peut les voir et signer.</summary>
    Task<IEnumerable<Assurance>> GetByIntermediaireIdAsync(Guid intermediaireId);
    Task<Assurance> CreateAsync(Assurance assurance);
    Task<Assurance> UpdateAsync(Assurance assurance);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Assurance?> GetByNoPoliceAsync(string noPolice);
    /// <summary>Retourne le NumeroCert le plus récent (trié DESC) pour la génération séquentielle.</summary>
    Task<string?> GetLastNumeroCertAsync();
    
    Task AddVisaAssuranceAsync(VisaAssurance visaAssurance);
    Task SaveChangesAsync();
}


