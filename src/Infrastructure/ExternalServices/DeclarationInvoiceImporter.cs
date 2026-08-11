using System.Net.Http.Json;
using AssuranceService.Application.Common;
using AssuranceService.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AssuranceService.Infrastructure.ExternalServices;

public sealed class DeclarationInvoiceImporter : IDeclarationInvoiceImporter
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
    private readonly IObjectStorageService _storage;
    private readonly IDocumentRepository _documentRepository;
    private readonly string _declarationBaseUrl;
    private readonly string _assuranceBucketName;

    public DeclarationInvoiceImporter(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IObjectStorageService storage,
        IDocumentRepository documentRepository,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _storage = storage;
        _documentRepository = documentRepository;
        _declarationBaseUrl = (configuration["DeclarationImportationService:BaseUrl"]
            ?? "http://dossierimportservice:8080").TrimEnd('/');
        _assuranceBucketName = configuration["MinIO:BucketName"] ?? "assurances";
    }

    public async Task<int> ImportAsync(
        Guid assuranceId,
        IReadOnlyCollection<DeclarationInvoiceSelection> selections,
        string user,
        CancellationToken cancellationToken = default)
    {
        await _storage.EnsureBucketExistsAsync(_assuranceBucketName, cancellationToken);
        var importedCount = 0;

        foreach (var selection in selections.GroupBy(item => item.DossierId).Select(group => group.First()))
        {
            var documents = await GetDocumentsAsync(selection.DossierId, cancellationToken);
            var commandIds = selection.CommandeIds.ToHashSet();
            var invoices = documents.Where(document =>
                    document.CommandeId.HasValue && commandIds.Contains(document.CommandeId.Value))
                .ToArray();

            // Les anciennes DI ont parfois enregistré la facture au niveau du dossier.
            if (invoices.Length == 0)
                invoices = documents.Where(document => !document.CommandeId.HasValue).ToArray();

            foreach (var invoice in invoices)
            {
                await ImportDocumentAsync(assuranceId, selection, invoice, user, cancellationToken);
                importedCount++;
            }
        }

        return importedCount;
    }

    private async Task<IReadOnlyCollection<DeclarationDocumentSource>> GetDocumentsAsync(
        Guid dossierId,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_declarationBaseUrl}/api/v1/dossiers/{dossierId}/documents");
        ForwardCurrentUserHeaders(request);
        using var response = await _httpClientFactory.CreateClient().SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DeclarationDocumentSource[]>(cancellationToken: cancellationToken)
            ?? [];
    }

    private async Task ImportDocumentAsync(
        Guid assuranceId,
        DeclarationInvoiceSelection selection,
        DeclarationDocumentSource source,
        string user,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_declarationBaseUrl}/api/v1/dossiers/{selection.DossierId}/documents/{source.Id}/download");
        ForwardCurrentUserHeaders(request);
        using var response = await _httpClientFactory.CreateClient().SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var originalFileName = SafeFileName(source.Description);
        var objectKey = $"assurances/{assuranceId}/di/{selection.DossierId}/{Guid.NewGuid():N}_{originalFileName}";
        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        await _storage.UploadAsync(
            _assuranceBucketName,
            objectKey,
            content,
            source.ContentType ?? response.Content.Headers.ContentType?.MediaType,
            cancellationToken);

        var description = $"Facture DI {selection.NumeroDI ?? selection.DossierId.ToString()} - {originalFileName}";
        if (description.Length > 255)
            description = description[..255];

        var now = DateTime.UtcNow;
        await _documentRepository.AddAsync(new Document
        {
            Id = Guid.NewGuid(),
            AssuranceId = assuranceId,
            TypeDocument = "FACTURE_DI",
            Description = description,
            DocumentUrl = objectKey,
            ContentType = source.ContentType ?? response.Content.Headers.ContentType?.MediaType,
            Taille = source.FileSize ?? response.Content.Headers.ContentLength,
            CreerPar = user,
            ModifierPar = user,
            CreerLe = now,
            ModifierLe = now
        }, cancellationToken);
    }

    private void ForwardCurrentUserHeaders(HttpRequestMessage request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
            return;

        foreach (var header in ForwardedHeaders)
        {
            if (context.Request.Headers.TryGetValue(header, out var value) && value.Count > 0)
                request.Headers.TryAddWithoutValidation(header, value.ToArray());
        }
    }

    private static string SafeFileName(string? value)
    {
        var fileName = Path.GetFileName(string.IsNullOrWhiteSpace(value) ? "facture" : value);
        var invalid = Path.GetInvalidFileNameChars();
        var safe = new string(fileName.Select(character => invalid.Contains(character) ? '_' : character).ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "facture" : safe;
    }

    private sealed record DeclarationDocumentSource(
        Guid Id,
        Guid DossierId,
        Guid? CommandeId,
        string? Description,
        string? ContentType,
        long? FileSize);
}
