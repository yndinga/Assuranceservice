using AssuranceService.Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AssuranceService.Infrastructure.Storage;

public sealed class LocalObjectStorageService : IObjectStorageService
{
    private readonly string _rootPath;
    private readonly ILogger<LocalObjectStorageService> _logger;

    public LocalObjectStorageService(IConfiguration configuration, ILogger<LocalObjectStorageService> logger)
    {
        _logger = logger;
        _rootPath = configuration["ObjectStorage:Local:RootPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "storage", "documents");
    }

    public Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(BucketPath(bucketName));
        return Task.CompletedTask;
    }

    public async Task UploadAsync(string bucketName, string objectName, Stream data, string? contentType = null, CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(bucketName, cancellationToken).ConfigureAwait(false);

        var fullPath = ObjectPath(bucketName, objectName);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        if (data.CanSeek)
            data.Position = 0;

        await using var fileStream = File.Create(fullPath);
        await data.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Stored {ObjectName} locally in bucket {BucketName}", objectName, bucketName);
    }

    public Task<Stream> GetObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        var fullPath = ObjectPath(bucketName, objectName);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Le fichier local est introuvable : {objectName}", fullPath);

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        var fullPath = ObjectPath(bucketName, objectName);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        _logger.LogInformation("Deleted local object {ObjectName} from bucket {BucketName}", objectName, bucketName);
        return Task.CompletedTask;
    }

    public Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expiresInSeconds = 3600, bool forUpload = false, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"local://{bucketName}/{NormalizeObjectName(objectName)}");
    }

    private string BucketPath(string bucketName)
    {
        var safeBucket = NormalizeSegment(bucketName);
        return Path.Combine(_rootPath, safeBucket);
    }

    private string ObjectPath(string bucketName, string objectName)
    {
        var bucketPath = BucketPath(bucketName);
        var relativeObjectPath = NormalizeObjectName(objectName).Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(bucketPath, relativeObjectPath));
        var safeRoot = Path.GetFullPath(bucketPath) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Chemin de fichier local invalide.");

        return fullPath;
    }

    private static string NormalizeObjectName(string objectName)
    {
        var parts = objectName
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeSegment);

        return string.Join('/', parts);
    }

    private static string NormalizeSegment(string segment)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(segment.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "_" : cleaned;
    }
}
