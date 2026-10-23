using StangaNetLib.Core.Common;

namespace StangaNetLib.EncryptedStorage.Errors;

internal static class EncryptedStorageErrors
{
    internal static Error PassphraseRequired()
        => Error.Validation("Passphrase", "A non-empty encryption passphrase is required.");

    internal static Error BlobNotFound(string blobId)
        => Error.NotFound("EncryptedBlob", blobId);

    internal static Error EncryptFailed(string description)
        => Error.Internal($"Encryption failed: {description}");

    internal static Error DecryptFailed(string description)
        => Error.Validation("Integrity", $"Decryption/integrity check failed: {description}");

    internal static Error InvalidBlobId()
        => Error.Validation("BlobId", "Blob id must not be null or whitespace.");
}
