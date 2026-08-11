using System.Net.Http.Json;
using AssuranceService.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AssuranceService.Infrastructure.ExternalServices;

public sealed class DeclarationDossierClient : IDeclarationDossierClient
{
    private static readonly string[] ForwardedHeaders =
    [
        "Authorization",
        "X-User-Name",
        "X-User-Code",
        "X-User-Roles",
        "X-Organisation-Code",
        "X-Organisation-Type"
    ];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _baseUrl;

    public DeclarationDossierClient(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _baseUrl = (configuration["DeclarationImportationService:BaseUrl"]
            ?? "http://dossierimportservice:8080").TrimEnd('/');
    }

    public async Task<DeclarationDossierSource?> GetDossierAsync(Guid dossierId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/v1/dossiers/{dossierId}");
        ForwardCurrentUserHeaders(request);

        using var response = await client.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DeclarationDossierSource>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyCollection<DeclarationDossierSource>> GetDossiersAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/v1/dossiers");
        ForwardCurrentUserHeaders(request);

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<DeclarationDossierSource>>(
                   cancellationToken: cancellationToken)
               ?? Array.Empty<DeclarationDossierSource>();
    }

    private void ForwardCurrentUserHeaders(HttpRequestMessage request)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return;

        foreach (var header in ForwardedHeaders)
        {
            if (!httpContext.Request.Headers.TryGetValue(header, out var value) || value.Count == 0)
                continue;

            request.Headers.TryAddWithoutValidation(header, value.ToArray());
        }
    }
}
