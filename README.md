# StangaNetLib.EncryptedStorage

[![.NET CI](https://github.com/StangaNet/StangaNetLib.EncryptedStorage/actions/workflows/main.yml/badge.svg)](https://github.com/StangaNet/StangaNetLib.EncryptedStorage/actions/workflows/main.yml)
![NuGet](https://img.shields.io/badge/nuget-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-512BD4)

Encrypted-at-rest blob storage for self-hosted apps. Composes [`StangaNetLib.Storage`](https://github.com/StangaNet/StangaNetLib.Storage) (Local) with [`StangaNetLib.Cryptography`](https://github.com/StangaNet/StangaNetLib.Cryptography) (**SNEN** AES-GCM envelopes). No outbound network is required.

## Installation

```xml
<PackageReference Include="StangaNetLib.EncryptedStorage" Version="1.0.0" />
```

## Envelope format

Ciphertext files use Cryptography **SNEN v1** (`SNEN/1`): magic, version, KDF id, salt, iterations, nonce, ciphertext, tag. Nonce/tag live inside the envelope — they are not duplicated in API metadata.

## Quick Start

```csharp
builder.Services.AddStangaNetLibEncryptedStorage(
    rootPath: "/var/app/blobs",
    passphrase: builder.Configuration["EncryptedStorageSettings:Passphrase"]!);

var store = sp.GetRequiredService<IEncryptedBlobStorage>();
await using var input = File.OpenRead("doc.pdf");
var uploaded = await store.UploadAsync(input, "application/pdf");
var verify = await store.VerifyAsync(uploaded.Value!.BlobId);
```

Exposed upload metadata includes blob id and **ciphertext** size only — never plaintext content.

## vs Storage EncryptAtRest

Storage's optional decorator decrypts transparently on download. This library exposes an explicit encrypted API (`VerifyAsync`, ciphertext size, envelope format) for apps that treat encryption as a first-class concern.

## License

Apache-2.0. See [LICENSE](./LICENSE).
