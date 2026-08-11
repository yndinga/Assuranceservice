using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AssuranceService.Application.Common;
using AssuranceService.Domain.Models.Referentiel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AssuranceService.Infrastructure.Messaging;

public sealed class ReferentielInitialSyncHostedService : IHostedService
{
    public const string HttpClientName = "ReferentielService";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ReferentielMessagingOptions _options;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReferentielInitialSyncHostedService> _logger;

    public ReferentielInitialSyncHostedService(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        IOptions<ReferentielMessagingOptions> options,
        IConfiguration configuration,
        ILogger<ReferentielInitialSyncHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.InitialSyncOnStartup) return;

        _logger.LogInformation("Sync initiale référentiel depuis {Url}", _options.ReferentielServiceBaseUrl);
        var http = _httpClientFactory.CreateClient(HttpClientName);
        AddServiceBearerToken(http);
        var baseUrl = _options.ReferentielServiceBaseUrl.TrimEnd('/');

        await using var scope = _scopeFactory.CreateAsyncScope();
        var sync = scope.ServiceProvider.GetRequiredService<IReferentielSyncService>();

        foreach (var entry in ReferentielSyncCatalog.All)
        {
            try
            {
                var response = await http.GetAsync($"{baseUrl}/api/{entry.ApiRoute}", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Sync {Route} ignorée : HTTP {Code}", entry.ApiRoute, (int)response.StatusCode);
                    continue;
                }

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                var count = 0;
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (!element.TryGetProperty("id", out var idProp)) continue;
                    await UpsertEntryAsync(sync, entry, idProp.GetGuid(), element.GetRawText(), cancellationToken);
                    count++;
                }

                _logger.LogInformation("Sync {Route} : {Count} enregistrement(s)", entry.ApiRoute, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Échec sync {Route}", entry.ApiRoute);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void AddServiceBearerToken(HttpClient http)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var signingKey = jwtSection["SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey))
            return;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims:
            [
                new Claim(ClaimTypes.Name, "AssuranceService"),
                new Claim("organisationCode", "SYSTEM"),
                new Claim("organisationType", "SYSTEM")
            ],
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);

        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
    }

    private static Task UpsertEntryAsync(
        IReferentielSyncService sync,
        ReferentielSyncEntry entry,
        Guid id,
        string json,
        CancellationToken ct) =>
        entry.LocalType.Name switch
        {
            nameof(Pays) => sync.UpsertPaysFromJsonAsync(id, json, ct),
            nameof(Aeroport) => sync.UpsertFromJsonAsync<Aeroport>(id, json, ct),
            nameof(Corridor) => sync.UpsertFromJsonAsync<Corridor>(id, json, ct),
            nameof(Departement) => sync.UpsertFromJsonAsync<Departement>(id, json, ct),
            nameof(Devise) => sync.UpsertFromJsonAsync<Devise>(id, json, ct),
            nameof(Etat) => sync.UpsertFromJsonAsync<Etat>(id, json, ct),
            nameof(Port) => sync.UpsertFromJsonAsync<Port>(id, json, ct),
            nameof(Route) => sync.UpsertFromJsonAsync<Route>(id, json, ct),
            nameof(RouteNationale) => sync.UpsertFromJsonAsync<RouteNationale>(id, json, ct),
            nameof(TauxDeChange) => sync.UpsertFromJsonAsync<TauxDeChange>(id, json, ct),
            nameof(Troncon) => sync.UpsertFromJsonAsync<Troncon>(id, json, ct),
            nameof(TypeTransport) => sync.UpsertFromJsonAsync<TypeTransport>(id, json, ct),
            nameof(UniteStatistique) => sync.UpsertFromJsonAsync<UniteStatistique>(id, json, ct),
            _ => throw new InvalidOperationException($"Type non géré : {entry.LocalType.Name}")
        };
}
