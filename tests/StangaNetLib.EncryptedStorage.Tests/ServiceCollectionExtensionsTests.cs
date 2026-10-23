using Microsoft.Extensions.DependencyInjection;
using StangaNetLib.EncryptedStorage.Abstractions;
using StangaNetLib.EncryptedStorage.Extensions;

namespace StangaNetLib.EncryptedStorage.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddStangaNetLibEncryptedStorage_RegistersService()
    {
        var root = Path.Combine(Path.GetTempPath(), "sn-enc-di-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var services = new ServiceCollection();
            services.AddStangaNetLibEncryptedStorage(root, "passphrase-for-di");
            var sp = services.BuildServiceProvider();

            sp.GetService<IEncryptedBlobStorage>().Should().NotBeNull();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
