using Microsoft.Extensions.Options;

namespace StangaNetLib.EncryptedStorage.Settings;

/// <summary>Validates <see cref="EncryptedStorageSettings"/>.</summary>
public sealed class EncryptedStorageSettingsValidator : IValidateOptions<EncryptedStorageSettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, EncryptedStorageSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.RootPath))
            return ValidateOptionsResult.Fail("RootPath must not be empty.");

        if (string.IsNullOrWhiteSpace(options.ContainerName))
            return ValidateOptionsResult.Fail("ContainerName must not be empty.");

        if (string.IsNullOrWhiteSpace(options.Passphrase))
            return ValidateOptionsResult.Fail("Passphrase must not be empty.");

        return ValidateOptionsResult.Success;
    }
}
