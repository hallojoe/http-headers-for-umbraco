using Casko.HttpHeadersForUmbraco.Package.Services;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Casko.HttpHeadersForUmbraco.Tests.Unit;

[TestFixture]
public sealed class ContentRootHttpResponseHeadersBindingFileResolverTests
{
    private readonly List<string> _directories = [];

    [TearDown]
    public void TearDown()
    {
        foreach (var path in _directories)
        {
            Directory.Delete(path, recursive: true);
        }
        _directories.Clear();
    }

    [Test]
    public void ResolvePath_WhenPathEscapesContentRoot_Throws()
    {
        var sut = CreateSut(CreateTempDirectory());

        Assert.Throws<InvalidOperationException>(() => sut.ResolvePath(Path.Combine("..", "outside.http")));
    }

    [Test]
    public void ResolvePath_WhenSiblingSharesRootPrefix_Throws()
    {
        var parent = CreateTempDirectory();
        var sut = CreateSut(Path.Combine(parent, "site"));

        Assert.Throws<InvalidOperationException>(() => sut.ResolvePath(Path.Combine("..", "site-other", "outside.http")));
    }

    [Test]
    public void ReadAsync_WhenSiblingDiffersOnlyByCase_ThrowsOnCaseSensitiveFileSystem()
    {
        var parent = CreateTempDirectory();
        var root = Path.Combine(parent, "site");
        var sibling = Path.Combine(parent, "SITE");
        Directory.CreateDirectory(root);
        if (Directory.Exists(sibling))
        {
            Assert.Ignore("This test requires a case-sensitive file system.");
        }
        Directory.CreateDirectory(sibling);
        File.WriteAllText(Path.Combine(sibling, "outside.http"), "X-Outside: yes");
        var sut = CreateSut(root);

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.ReadAsync(Path.Combine("..", "SITE", "outside.http")));
    }

    [Test]
    public async Task ReadAsync_WhenFileExists_ReadsContents()
    {
        var rootPath = CreateTempDirectory();
        var filePath = Path.Combine(rootPath, "headers.http");
        await File.WriteAllTextAsync(filePath, "X-Test: abc");

        var sut = CreateSut(rootPath);

        var result = await sut.ReadAsync("headers.http");

        Assert.That(result, Is.EqualTo("X-Test: abc"));
    }

    private static ContentRootHttpResponseHeadersBindingFileResolver CreateSut(string rootPath)
    {
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        hostEnvironment.ContentRootPath.Returns(rootPath);
        return new ContentRootHttpResponseHeadersBindingFileResolver(hostEnvironment);
    }

    private string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        _directories.Add(path);
        return path;
    }
}
