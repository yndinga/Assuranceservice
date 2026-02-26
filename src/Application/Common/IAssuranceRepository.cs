using AssuranceService.Domain.Models;

namespace AssuranceService.Application.Common;

public interface IAssuranceRepository
{
    Task<Assurance?> GetByIdAsync(Guid id);
    /// <summary>Récupère uniquement le statut (requête simple, évite les problèmes de projection).</summary>
    Task<string?> GetStatutAsync(Guid id);
    Task<IEnumerable<Assurance>> GetAllAsync();
    Task<Assurance> CreateAsync(Assurance assurance);
    Task<Assurance> UpdateAsync(Assurance assurance);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Assurance?> GetByNoPoliceAsync(string noPolice);
    /// <summary>Retourne le NumeroCert le plus récent (trié DESC) pour la génération séquentielle.</summary>
    Task<string?> GetLastNumeroCertAsync();
    
    Task AddVoyageAsync(Voyage voyage);
    Task AddVisaAssuranceAsync(VisaAssurance visaAssurance);
    Task SaveChangesAsync();
}


