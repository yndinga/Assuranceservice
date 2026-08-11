using AssuranceService.Application.Common;
using Microsoft.AspNetCore.Http;
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
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _partenaireServiceUrl;

    public PartenaireService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        var configuredUrl = configuration["ExternalServices:PartenaireServiceUrl"]
            ?? throw new InvalidOperationException("PartenaireServiceUrl not configured");

        // L'adresse localhost du fichier appsettings ne peut pas fonctionner depuis Portainer.
        // En conteneur, la Gateway reste le point d'entree de secours si la variable de stack est absente.
        if (string.Equals(
                Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
                "true",
                StringComparison.OrdinalIgnoreCase)
            && configuredUrl.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase))
        {
            configuredUrl = configuration["ExternalServices:OrganisationGatewayUrl"]
                ?? "http://192.168.2.89:5000/organisation";
        }

        _partenaireServiceUrl = configuredUrl.TrimEnd('/');
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
                BuildOrganisationUrl(code));

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
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                ForwardCurrentUserHeaders(request);
                return await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException) when (attempt < ConnectionRetryDelays.Length)
            {
                await Task.Delay(ConnectionRetryDelays[attempt]);
            }
        }
    }

    private string BuildOrganisationUrl(string escapedCode)
    {
        // Via la Gateway, /organisation/{everything} est traduit en /api/v1/{everything}.
        if (_partenaireServiceUrl.EndsWith("/organisation", StringComparison.OrdinalIgnoreCase))
            return $"{_partenaireServiceUrl}/organisations/{escapedCode}";

        return $"{_partenaireServiceUrl}/api/v1/organisations/{escapedCode}";
    }

    private void ForwardCurrentUserHeaders(HttpRequestMessage request)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return;

        foreach (var header in new[]
                 {
                     "Authorization",
                     "X-User-Name",
                     "X-User-Code",
                     "X-User-Roles",
                     "X-Organisation-Code",
                     "X-Organisation-Type"
                 })
        {
            if (httpContext.Request.Headers.TryGetValue(header, out var value) && value.Count > 0)
                request.Headers.TryAddWithoutValidation(header, value.ToArray());
        }
    }
}



