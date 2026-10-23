using System.Text;
using Microsoft.Extensions.DependencyInjection;
using StangaNetLib.EncryptedStorage.Abstractions;
using StangaNetLib.EncryptedStorage.Extensions;

namespace StangaNetLib.EncryptedStorage.Tests;

public class EncryptedBlobStorageTests
{
    private static (IEncryptedBlobStorage Store, string Root) CreateSut(string? passphrase = "test-passphrase-ok")
    {
        var root = Path.Combine(Path.GetTempPath(), "sn-enc-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var services = new ServiceCollection();
        services.AddStangaNetLibEncryptedStorage(root, passphrase!, "encrypted");
        var sp = services.BuildServiceProvider();
        return (sp.GetRequiredService<IEncryptedBlobStorage>(), root);
    }

    [Fact]
    public async Task RoundTrip_UploadDownload_ShouldRestorePlaintext()
    {
        var (store, root) = CreateSut();
        try
        {
            var payload = Encoding.UTF8.GetBytes("hello encrypted world");
            await using var input = new MemoryStream(payload);

            var upload = await store.UploadAsync(input, "text/plain", "doc1");
            upload.IsSuccess.Should().BeTrue();
            upload.Value!.BlobId.Should().Be("doc1");
            upload.Value.CiphertextSizeBytes.Should().BeGreaterThan(payload.Length);
            upload.Value.EnvelopeFormat.Should().Be("SNEN/1");
            upload.Value.ContentType.Should().Be("text/plain");

            var download = await store.DownloadAsync("doc1");
            download.IsSuccess.Should().BeTrue();
            await using (download.Value!.Content)
            {
                using var reader = new StreamReader(download.Value.Content);
                (await reader.ReadToEndAsync()).Should().Be("hello encrypted world");
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Verify_OkAndWrongPassphrase()
    {
        var root = Path.Combine(Path.GetTempPath(), "sn-enc-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var services = new ServiceCollection();
            services.AddStangaNetLibEncryptedStorage(root, "correct-secret");
            var sp = services.BuildServiceProvider();
            var store = sp.GetRequiredService<IEncryptedBlobStorage>();

            await using (var input = new MemoryStream(Encoding.UTF8.GetBytes("secret-data")))
            {
                (await store.UploadAsync(input, "text/plain", "a")).IsSuccess.Should().BeTrue();
            }

            (await store.VerifyAsync("a")).IsSuccess.Should().BeTrue();

            var wrong = new ServiceCollection();
            wrong.AddStangaNetLibEncryptedStorage(root, "wrong-secret");
            var wrongStore = wrong.BuildServiceProvider().GetRequiredService<IEncryptedBlobStorage>();

            (await wrongStore.VerifyAsync("a")).IsFailure.Should().BeTrue();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ExistsAndDelete_ShouldWork()
    {
        var (store, root) = CreateSut();
        try
        {
            await using (var input = new MemoryStream([1, 2, 3]))
            {
                await store.UploadAsync(input, "application/octet-stream", "x");
            }

            (await store.ExistsAsync("x")).Should().BeTrue();
            (await store.DeleteAsync("x")).IsSuccess.Should().BeTrue();
            (await store.ExistsAsync("x")).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task UploadResult_ShouldNotExposePlaintext()
    {
        var (store, root) = CreateSut();
        try
        {
            var secret = "TOP-SECRET-CONTENT";
            await using var input = new MemoryStream(Encoding.UTF8.GetBytes(secret));
            var upload = await store.UploadAsync(input, "text/plain");

            var dumped = upload.Value!.ToString();
            dumped.Should().NotContain(secret);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
