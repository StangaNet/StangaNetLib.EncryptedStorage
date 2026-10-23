namespace StangaNetLib.EncryptedStorage.Models;

/// <summary>
/// Decrypted download result. The caller owns and must dispose <see cref="Content"/>.
/// </summary>
/// <param name="BlobId">Blob identifier.</param>
/// <param name="Content">Plaintext stream (caller disposes).</param>
/// <param name="ContentType">Original content type.</param>
/// <param name="PlaintextSizeBytes">Decrypted payload size.</param>
public sealed record EncryptedBlobDownloadResult(
    string BlobId,
    Stream Content,
    string ContentType,
    long PlaintextSizeBytes);
