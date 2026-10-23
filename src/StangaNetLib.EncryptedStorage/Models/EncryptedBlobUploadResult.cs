namespace StangaNetLib.EncryptedStorage.Models;

/// <summary>
/// Result of an encrypted upload. Does not include plaintext content.
/// </summary>
/// <param name="BlobId">Blob identifier (storage key).</param>
/// <param name="CiphertextSizeBytes">Size of the SNEN envelope on disk.</param>
/// <param name="ContentType">Original (plaintext) content type stored in sidecar metadata only.</param>
/// <param name="EnvelopeFormat">Envelope identifier, always <c>SNEN/1</c> in v1.</param>
public sealed record EncryptedBlobUploadResult(
    string BlobId,
    long CiphertextSizeBytes,
    string ContentType,
    string EnvelopeFormat = "SNEN/1");
