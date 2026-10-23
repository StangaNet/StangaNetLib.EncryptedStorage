using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StangaNetLib.Cryptography.Extensions;
using StangaNetLib.EncryptedStorage.Abstractions;
using StangaNetLib.EncryptedStorage.Implementations;
using StangaNetLib.EncryptedStorage.Settings;
using StangaNetLib.Storage.Extensions;

namespace StangaNetLib.EncryptedStorage.Extensions;

/// <summary>DI registration for StangaNetLib.EncryptedStorage.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers encrypted blob storage from the <c>EncryptedStorageSettings</c> section.
    /// Registers Cryptography and Storage Local (at <c>RootPath</c>).
    /// </summary>
    public static IServiceCollection AddStangaNetLibEncryptedStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(EncryptedStorageSettings.SectionName);
        services.Configure<EncryptedStorageSettings>(section);

        var rootPath = section["RootPath"];
        if (string.IsNullOrWhiteSpace(rootPath))
            rootPath = new EncryptedStorageSettings().RootPath;

        services.AddStangaNetLibCryptography(configuration);
        services.AddStangaNetLibStorageLocal(rootPath);
        services.AddSingleton<IValidateOptions<EncryptedStorageSettings>, EncryptedStorageSettingsValidator>();
        services.TryAddSingleton<IEncryptedBlobStorage, EncryptedBlobStorage>();
        return services;
    }

    /// <summary>
    /// Registers encrypted blob storage with an explicit root path and passphrase.
    /// Registers Cryptography (defaults) and Storage Local at <paramref name="rootPath"/>.
    /// </summary>
    public static IServiceCollection AddStangaNetLibEncryptedStorage(
        this IServiceCollection services,
        string rootPath,
        string passphrase,
        string containerName = "encrypted")
    {
        services.Configure<EncryptedStorageSettings>(opts =>
        {
            opts.RootPath = rootPath;
            opts.Passphrase = passphrase;
            opts.ContainerName = containerName;
        });

        services.AddStangaNetLibCryptography();
        services.AddStangaNetLibStorageLocal(rootPath);
        services.AddSingleton<IValidateOptions<EncryptedStorageSettings>, EncryptedStorageSettingsValidator>();
        services.TryAddSingleton<IEncryptedBlobStorage, EncryptedBlobStorage>();
        return services;
    }
}
