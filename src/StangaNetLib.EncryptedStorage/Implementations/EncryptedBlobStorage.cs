using Microsoft.Extensions.Options;
using StangaNetLib.Core.Common;
using StangaNetLib.Core.Guards;
using StangaNetLib.Cryptography.Abstractions;
using StangaNetLib.EncryptedStorage.Abstractions;
using StangaNetLib.EncryptedStorage.Errors;
using StangaNetLib.EncryptedStorage.Models;
using StangaNetLib.EncryptedStorage.Settings;
using StangaNetLib.Storage.Abstractions;

namespace StangaNetLib.EncryptedStorage.Implementations;

/// <summary>
/// Composes <see cref="IBlobStorageService"/> (Local) with <see cref="IAtRestEncryptionService"/> (SNEN).
/// </summary>
public sealed class EncryptedBlobStorage : IEncryptedBlobStorage
{
    internal const string OriginalContentTypeMetadataKey = "x-sn-enc-original-content-type";
    internal const string EnvelopeFormat = "SNEN/1";

    private readonly IBlobStorageService _storage;
    private readonly IAtRestEncryptionService _encryption;
    private readonly EncryptedStorageSettings _settings;

    /// <summary>Creates the encrypted blob storage service.</summary>
    public EncryptedBlobStorage(
        IBlobStorageService storage,
        IAtRestEncryptionService encryption,
        IOptions<EncryptedStorageSettings> settings)
    {
        _storage = Guard.Against.Null(storage, nameof(storage));
        _encryption = Guard.Against.Null(encryption, nameof(encryption));
        Guard.Against.Null(settings, nameof(settings));
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public async Task<Result<EncryptedBlobUploadResult>> UploadAsync(
        Stream content,
        string contentType,
        string? blobId = null,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(content, nameof(content));
        Guard.Against.NullOrWhiteSpace(contentType, nameof(contentType));

        if (string.IsNullOrWhiteSpace(_settings.Passphrase))
            return Result<EncryptedBlobUploadResult>.Failure(EncryptedStorageErrors.PassphraseRequired());

        var id = string.IsNullOrWhiteSpace(blobId) ? Guid.NewGuid().ToString("N") : blobId.Trim();

        byte[] plaintext;
        await using (var buffer = new MemoryStream())
        {
            await content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
            plaintext = buffer.ToArray();
        }

        var encrypt = _encryption.EncryptBlob(plaintext, _settings.Passphrase);
        if (encrypt.IsFailure)
            return Result<EncryptedBlobUploadResult>.Failure(
                EncryptedStorageErrors.EncryptFailed(encrypt.Error.Description));

        var ciphertext = encrypt.Value!;
        await using var cipherStream = new MemoryStream(ciphertext, writable: false);

        var metadata = new Dictionary<string, string>
        {
            [OriginalContentTypeMetadataKey] = contentType
        };

        var upload = await _storage.UploadAsync(
                _settings.ContainerName,
                id,
                cipherStream,
                "application/octet-stream",
                metadata,
                cancellationToken)
            .ConfigureAwait(false);

        if (upload.IsFailure)
            return Result<EncryptedBlobUploadResult>.Failure(upload.Error);

        return Result<EncryptedBlobUploadResult>.Success(
            new EncryptedBlobUploadResult(
                id,
                ciphertext.LongLength,
                contentType,
                EnvelopeFormat));
    }

    /// <inheritdoc />
    public async Task<Result<EncryptedBlobDownloadResult>> DownloadAsync(
        string blobId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobId))
            return Result<EncryptedBlobDownloadResult>.Failure(EncryptedStorageErrors.InvalidBlobId());

        if (string.IsNullOrWhiteSpace(_settings.Passphrase))
            return Result<EncryptedBlobDownloadResult>.Failure(EncryptedStorageErrors.PassphraseRequired());

        var download = await _storage
            .DownloadAsync(_settings.ContainerName, blobId, cancellationToken)
            .ConfigureAwait(false);

        if (download.IsFailure)
            return Result<EncryptedBlobDownloadResult>.Failure(
                download.Error.Type == ErrorType.NotFound
                    ? EncryptedStorageErrors.BlobNotFound(blobId)
                    : download.Error);

        await using var cipherStream = download.Value!.Content;
        byte[] envelope;
        await using (var buffer = new MemoryStream())
        {
            await cipherStream.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
            envelope = buffer.ToArray();
        }

        var decrypt = _encryption.DecryptBlob(envelope, _settings.Passphrase);
        if (decrypt.IsFailure)
            return Result<EncryptedBlobDownloadResult>.Failure(
                EncryptedStorageErrors.DecryptFailed(decrypt.Error.Description));

        var plaintext = decrypt.Value!;
        var contentType = download.Value.Metadata.TryGetValue(OriginalContentTypeMetadataKey, out var ct)
            ? ct
            : "application/octet-stream";

        Stream content = new MemoryStream(plaintext, writable: false);
        return Result<EncryptedBlobDownloadResult>.Success(
            new EncryptedBlobDownloadResult(blobId, content, contentType, plaintext.LongLength));
    }

    /// <inheritdoc />
    public Task<Result> DeleteAsync(string blobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobId))
            return Task.FromResult(Result.Failure(EncryptedStorageErrors.InvalidBlobId()));

        return _storage.DeleteAsync(_settings.ContainerName, blobId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string blobId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobId))
            return Task.FromResult(false);

        return _storage.ExistsAsync(_settings.ContainerName, blobId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result> VerifyAsync(string blobId, CancellationToken cancellationToken = default)
    {
        var download = await DownloadAsync(blobId, cancellationToken).ConfigureAwait(false);
        if (download.IsFailure)
            return Result.Failure(download.Error);

        await download.Value!.Content.DisposeAsync().ConfigureAwait(false);
        return Result.Success();
    }
}
