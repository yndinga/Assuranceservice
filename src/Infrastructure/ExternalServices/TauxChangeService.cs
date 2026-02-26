using AssuranceService.Application.Common;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace AssuranceService.Infrastructure.ExternalServices;

/// <summary>
/// Service pour appeler le microservice TauxChange
/// </summary>
public class TauxChangeService : ITauxChangeService
{
    private readonly HttpClient _httpClient;
    private readonly string _tauxChangeServiceUrl;

    public TauxChangeService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _tauxChangeServiceUrl = configuration["ExternalServices:TauxChangeServiceUrl"] 
            ?? throw new InvalidOperationException("TauxChangeServiceUrl not configured");
    }

    public async Task<decimal> GetTauxChangeAsync(string devise, DateTime? date = null)
    {
        try
        {
            var dateParam = date?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");
            var url = $"{_tauxChangeServiceUrl}/api/tauxchange/{devise}?date={dateParam}";
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException($"Taux de change pour la devise {devise} introuvable");
                }
                
                throw new HttpRequestException($"Erreur lors de l'appel au service TauxChange: {response.StatusCode}");
            }

            var tauxChangeDto = await response.Content.ReadFromJsonAsync<TauxChangeDto>();
            
            if (tauxChangeDto == null)
            {
                throw new InvalidOperationException($"Réponse invalide du service TauxChange pour la devise {devise}");
            }

            return tauxChangeDto.Taux;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Impossible de contacter le service TauxChange: {ex.Message}", ex);
        }
    }
}



