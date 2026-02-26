using AssuranceService.Application.Common;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace AssuranceService.Infrastructure.ExternalServices;

/// <summary>
/// Service pour appeler le microservice Partenaires
/// </summary>
public class PartenaireService : IPartenaireService
{
    private readonly HttpClient _httpClient;
    private readonly string _partenaireServiceUrl;

    public PartenaireService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _partenaireServiceUrl = configuration["ExternalServices:PartenaireServiceUrl"] 
            ?? throw new InvalidOperationException("PartenaireServiceUrl not configured");
    }

    public async Task<string> GetCodePartenaireAsync(Guid partenaireId)
    {
        var partenaire = await GetPartenaireAsync(partenaireId);
        
        if (partenaire == null)
        {
            throw new InvalidOperationException($"Partenaire {partenaireId} introuvable");
        }

        return partenaire.Code;
    }

    public async Task<PartenaireDto?> GetPartenaireAsync(Guid partenaireId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_partenaireServiceUrl}/api/partenaires/{partenaireId}");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                
                throw new HttpRequestException($"Erreur lors de l'appel au service Partenaires: {response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<PartenaireDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Impossible de contacter le service Partenaires: {ex.Message}", ex);
        }
    }
}



