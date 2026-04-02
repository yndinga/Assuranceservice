using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AssuranceService.Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace AssuranceService.Infrastructure.Storage;

/// <summary>
/// Stockage documents "mode wrapper" :
/// - upload via api/assurance-documents (wrapper HTTP, multi-part/form-data)
/// - puis utilise MinIO direct pour presigner / download / delete
/// </summary>
public class AssuranceDocumentsWrapperObjectStorageService : IObjectStorageService
{
    private readonly HttpClient _httpClient;
    private readonly string _wrapperUploadUrl;
    private readonly string? _wrapperUsername;
    private readonly string? _wrapperPassword;
    private readonly IMinioClient _minioClient;
    private readonly ILogger<AssuranceDocumentsWrapperObjectStorageService> _logger;

    public AssuranceDocumentsWrapperObjectStorageService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AssuranceDocumentsWrapperObjectStorageService> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(nameof(AssuranceDocumentsWrapperObjectStorageService));

        var wrapperBaseUrl = configuration["DocumentWrapper:BaseUrl"];
        if (string.IsNullOrWhiteSpace(wrapperBaseUrl))
            throw new InvalidOperationException("DocumentWrapper:BaseUrl manquant (configuration).");

        var uploadPath = configuration["DocumentWrapper:UploadPath"] ?? "/api/assurance-documents";
        _wrapperUploadUrl = $"{wrapperBaseUrl.TrimEnd('/')}/{uploadPath.TrimStart('/')}";
        _wrapperUsername = configuration["DocumentWrapper:Username"];
        _wrapperPassword = configuration["DocumentWrapper:Password"];

        // MinIO client (pour presigned URL et suppression d'objet)
        var endpoint = configuration["MinIO:Endpoint"] ?? "localhost:9000";
        var accessKey = configuration["MinIO:AccessKey"] ?? "minioadmin";
        var secretKey = configuration["MinIO:SecretKey"] ?? "minioadmin";
        var useSsl = configuration.GetValue<bool>("MinIO:UseSSL");

        _minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(useSsl)
            .Build();
    }

    public async Task UploadAsync(
        string bucketName,
        string objectName,
        Stream data,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        // Le wrapper a besoin de "assuranceID" et "files" uniquement.
        // On en déduit assuranceID depuis la clé objectName envoyée par l'API.
        if (!TryExtractAssuranceId(objectName, out var assuranceId))
            throw new InvalidOperationException($"Impossible d'extraire assuranceID depuis objectName: '{objectName}'.");

        var fileName = ExtractFileNameFromObjectName(objectName);

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(assuranceId), "assuranceID");

        var streamContent = new StreamContent(data);
        if (!string.IsNullOrWhiteSpace(contentType))
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        // Le wrapper attend un champ formulaire "files"
        form.Add(streamContent, "files", fileName);

        using var request = new HttpRequestMessage(HttpMethod.Post, _wrapperUploadUrl)
        {
            Content = form
        };

        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(_wrapperUsername) && !string.IsNullOrWhiteSpace(_wrapperPassword))
        {
            var authBytes = Encoding.UTF8.GetBytes($"{_wrapperUsername}:{_wrapperPassword}");
            var authB64 = Convert.ToBase64String(authBytes);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authB64);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Erreur wrapper upload documents: HTTP {(int)response.StatusCode}. Réponse: {body}");

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var parsed = JsonSerializer.Deserialize<AssuranceDocumentsUploadResponse>(body, options);
        if (parsed?.Documents == null || parsed.Documents.Count == 0)
            throw new InvalidOperationException($"Réponse wrapper inattendue (aucun document). Body: {body}");

        // Le microservice upload 1 fichier par appel, donc on attend 1 document retourné.
        if (parsed.Documents.Count != 1)
            _logger.LogWarning("Wrapper a retourné {Count} documents pour un seul fichier envoyé.", parsed.Documents.Count);

        // Le wrapper réalise l'upload ; la clé d'objet conservée par l'API reste objectName.
        _logger.LogInformation("Upload wrapper OK pour assurance {AssuranceId}, objet {ObjectName}.", assuranceId, objectName);
    }

    public async Task<Stream> GetObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        var memory = new MemoryStream();
        await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => { stream.CopyTo(memory); }),
            cancellationToken)
            .ConfigureAwait(false);

        memory.Position = 0;
        return memory;
    }

    public async Task DeleteAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        await _minioClient.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<string> GetPresignedUrlAsync(
        string bucketName,
        string objectName,
        int expiresInSeconds = 3600,
        bool forUpload = false,
        CancellationToken cancellationToken = default)
    {
        var expirySeconds = Math.Max(1, Math.Min(expiresInSeconds, 604800)); // 1s .. 7 jours
        if (forUpload)
        {
            return await _minioClient.PresignedPutObjectAsync(new PresignedPutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithExpiry(expirySeconds))
                .ConfigureAwait(false);
        }

        return await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry(expirySeconds))
            .ConfigureAwait(false);
    }

    public async Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        var exists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName), cancellationToken)
            .ConfigureAwait(false);
        if (!exists)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName), cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static string ExtractFileNameFromObjectName(string objectName)
    {
        // objectName exemple : assurances/{assuranceId}/{guid}_{fileName}
        var parts = objectName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? "file" : parts[^1];
    }

    private static bool TryExtractAssuranceId(string objectName, out string assuranceId)
    {
        assuranceId = string.Empty;

        var parts = objectName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 &&
            parts[0].Equals("assurances", StringComparison.OrdinalIgnoreCase) &&
            Guid.TryParse(parts[1], out _))
        {
            assuranceId = parts[1];
            return true;
        }

        // Fallback : chercher le premier GUID dans la chaîne.
        foreach (var p in parts)
        {
            if (Guid.TryParse(p, out _))
            {
                assuranceId = p;
                return true;
            }
        }

        return false;
    }

    private sealed class AssuranceDocumentsUploadResponse
    {
        public string? Message { get; set; }
        public List<AssuranceDocumentUploadDto>? Documents { get; set; }
    }

    private sealed class AssuranceDocumentUploadDto
    {
        public string CleObjet { get; set; } = string.Empty;
    }
}

