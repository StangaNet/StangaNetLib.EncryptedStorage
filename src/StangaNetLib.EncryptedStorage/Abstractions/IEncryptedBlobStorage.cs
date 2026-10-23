using StangaNetLib.Core.Common;
using StangaNetLib.EncryptedStorage.Models;

namespace StangaNetLib.EncryptedStorage.Abstractions;

/// <summary>
/// Blob storage with encryption at rest (SNEN envelope via Cryptography).
/// Exposed metadata never includes plaintext content.
/// </summary>
public interface IEncryptedBlobStorage
{
    /// <summary>Encrypts and stores a blob. Generates a GUID id when <paramref name="blobId"/> is null.</summary>
    Task<Result<EncryptedBlobUploadResult>> UploadAsync(
        Stream content,
        string contentType,
        string? blobId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Downloads and decrypts a blob. Integrity failures return a failed <see cref="Result{T}"/>.</summary>
    Task<Result<EncryptedBlobDownloadResult>> DownloadAsync(
        string blobId,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes the ciphertext blob.</summary>
    Task<Result> DeleteAsync(string blobId, CancellationToken cancellationToken = default);

    /// <summary>Returns whether a ciphertext blob exists.</summary>
    Task<bool> ExistsAsync(string blobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies that the blob decrypts successfully without returning plaintext to the caller.
    /// </summary>
    Task<Result> VerifyAsync(string blobId, CancellationToken cancellationToken = default);
}
