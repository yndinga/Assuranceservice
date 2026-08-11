using AssuranceService.Application.Common;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace AssuranceService.Infrastructure.ExternalServices;

/// <summary>
/// Service pour appeler le microservice Partenaires
/// </summary>
public class PartenaireService : IPartenaireService
{
    private static readonly TimeSpan[] ConnectionRetryDelays =
    [
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2)
    ];

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
            using var response = await GetWithConnectionRetryAsync(
                $"{_partenaireServiceUrl}/api/partenaires/{partenaireId}");
            
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

    public async Task<OrganisationDto?> GetOrganisationAsync(string organisationCode)
    {
        if (string.IsNullOrWhiteSpace(organisationCode))
        {
            return null;
        }

        try
        {
            var code = Uri.EscapeDataString(organisationCode.Trim());
            using var response = await GetWithConnectionRetryAsync(
                $"{_partenaireServiceUrl}/api/v1/organisations/{code}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw new HttpRequestException($"Erreur lors de l'appel au service Organisations: {response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<OrganisationDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Impossible de contacter le service Organisations ({_partenaireServiceUrl}): {ex.Message}",
                ex);
        }
    }

    private async Task<HttpResponseMessage> GetWithConnectionRetryAsync(string url)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await _httpClient.GetAsync(url);
            }
            catch (HttpRequestException) when (attempt < ConnectionRetryDelays.Length)
            {
                await Task.Delay(ConnectionRetryDelays[attempt]);
            }
        }
    }
}



