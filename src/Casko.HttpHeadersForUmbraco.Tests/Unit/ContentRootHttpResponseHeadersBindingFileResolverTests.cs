using Casko.HttpHeadersForUmbraco.Package.Services;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Casko.HttpHeadersForUmbraco.Tests.Unit;

[TestFixture]
public sealed class ContentRootHttpResponseHeadersBindingFileResolverTests
{
    [Test]
    public void ResolvePath_WhenPathEscapesContentRoot_Throws()
    {
        var sut = CreateSut(CreateTempDirectory());

        Assert.Throws<InvalidOperationException>(() => sut.ResolvePath("..\\outside.http"));
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

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
