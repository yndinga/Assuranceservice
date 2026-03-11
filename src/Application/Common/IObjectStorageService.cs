namespace AssuranceService.Application.Common;

/// <summary>
/// Service de stockage d'objets (MinIO / S3-compatible).
/// </summary>
public interface IObjectStorageService
{
    /// <summary>
    /// Envoie un fichier vers le bucket.
    /// </summary>
    /// <param name="bucketName">Nom du bucket</param>
    /// <param name="objectName">Nom de l'objet (clé)</param>
    /// <param name="data">Contenu du fichier</param>
    /// <param name="contentType">Type MIME (ex: application/pdf)</param>
    /// <param name="cancellationToken">Annulation</param>
    Task UploadAsync(string bucketName, string objectName, Stream data, string? contentType = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Télécharge un objet sous forme de stream.
    /// </summary>
    Task<Stream> GetObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un objet.
    /// </summary>
    Task DeleteAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Génère une URL pré-signée pour téléchargement (GET) ou upload (PUT).
    /// </summary>
    /// <param name="bucketName">Nom du bucket</param>
    /// <param name="objectName">Nom de l'objet</param>
    /// <param name="expiresInSeconds">Durée de validité en secondes</param>
    /// <param name="forUpload">Si true, URL pour PUT; sinon pour GET</param>
    Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expiresInSeconds = 3600, bool forUpload = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifie que le bucket existe, le crée si nécessaire.
    /// </summary>
    Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken = default);
}
