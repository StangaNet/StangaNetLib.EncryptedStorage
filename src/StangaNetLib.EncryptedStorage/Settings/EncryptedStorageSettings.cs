namespace StangaNetLib.EncryptedStorage.Settings;

/// <summary>Configuration for encrypted local blob storage.</summary>
public sealed class EncryptedStorageSettings
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "EncryptedStorageSettings";

    /// <summary>Local filesystem root for the Storage Local provider.</summary>
    public string RootPath { get; set; } = "wwwroot/encrypted-storage";

    /// <summary>Storage container name (subdirectory under root).</summary>
    public string ContainerName { get; set; } = "encrypted";

    /// <summary>Passphrase used for SNEN PBKDF2 key derivation. Never log this value.</summary>
    public string Passphrase { get; set; } = string.Empty;
}
