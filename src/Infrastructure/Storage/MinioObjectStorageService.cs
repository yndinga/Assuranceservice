using AssuranceService.Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace AssuranceService.Infrastructure.Storage;

public class MinioObjectStorageService : IObjectStorageService
{
    private readonly IMinioClient _client;
    private readonly ILogger<MinioObjectStorageService> _logger;

    public MinioObjectStorageService(IConfiguration configuration, ILogger<MinioObjectStorageService> logger)
    {
        _logger = logger;
        var endpoint = configuration["MinIO:Endpoint"] ?? "localhost:9000";
        var accessKey = configuration["MinIO:AccessKey"] ?? "minioadmin";
        var secretKey = configuration["MinIO:SecretKey"] ?? "minioadmin";
        var useSsl = configuration.GetValue<bool>("MinIO:UseSSL");

        _client = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(useSsl)
            .Build();
    }

    public async Task UploadAsync(string bucketName, string objectName, Stream data, string? contentType = null, CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(bucketName, cancellationToken);
        var size = data.Length;
        var args = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(size);
        if (!string.IsNullOrEmpty(contentType))
            args.WithContentType(contentType);
        await _client.PutObjectAsync(args, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Uploaded {ObjectName} to bucket {BucketName}", objectName, bucketName);
    }

    public async Task<Stream> GetObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        var memory = new MemoryStream();
        var args = new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream =>
            {
                stream.CopyTo(memory);
            });
        await _client.GetObjectAsync(args, cancellationToken).ConfigureAwait(false);
        memory.Position = 0;
        return memory;
    }

    public async Task DeleteAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        await _client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(bucketName).WithObject(objectName), cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Removed {ObjectName} from bucket {BucketName}", objectName, bucketName);
    }

    public async Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expiresInSeconds = 3600, bool forUpload = false, CancellationToken cancellationToken = default)
    {
        var expirySeconds = Math.Max(1, Math.Min(expiresInSeconds, 604800)); // 1s .. 7 jours
        if (forUpload)
        {
            return await _client.PresignedPutObjectAsync(new PresignedPutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry(expirySeconds)).ConfigureAwait(false);
        }
        return await _client.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry(expirySeconds)).ConfigureAwait(false);
    }

    public async Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName), cancellationToken).ConfigureAwait(false);
        if (!exists)
        {
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName), cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Created bucket {BucketName}", bucketName);
        }
    }
}
