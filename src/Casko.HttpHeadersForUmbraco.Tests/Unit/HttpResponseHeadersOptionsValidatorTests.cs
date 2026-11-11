using Casko.HttpHeadersForUmbraco.Package.Configuration;
using Casko.HttpHeadersForUmbraco.Package.Services;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Casko.HttpHeadersForUmbraco.Tests.Unit;

[TestFixture]
public sealed class HttpResponseHeadersOptionsValidatorTests
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
    public void Validate_WhenFileIsMissing_ReturnsFailure()
    {
        var sut = CreateSut(CreateTempDirectory());

        var result = sut.Validate(null, CreateOptions(["missing.http"]));

        Assert.That(result.Failed, Is.True);
    }

    [Test]
    public void Validate_WhenFileReferenceIsEmpty_ReturnsFailure()
    {
        var sut = CreateSut(CreateTempDirectory());

        var result = sut.Validate(null, CreateOptions([" "]));

        Assert.That(result.Failed, Is.True);
    }

    [Test]
    public void Validate_WhenFileEscapesContentRoot_ReturnsFailure()
    {
        var root = CreateTempDirectory();
        var outsideFile = Path.Combine(CreateTempDirectory(), "outside.http");
        File.WriteAllText(outsideFile, "X-Outside: yes");
        var sut = CreateSut(root);

        var result = sut.Validate(null, CreateOptions([Path.GetRelativePath(root, outsideFile)]));

        Assert.That(result.Failed, Is.True);
        Assert.That(result.FailureMessage, Does.Contain("must resolve inside the content root"));
    }

    private static HttpResponseHeadersOptionsValidator CreateSut(string rootPath)
    {
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        hostEnvironment.ContentRootPath.Returns(rootPath);
        var resolver = new ContentRootHttpResponseHeadersBindingFileResolver(hostEnvironment);
        return new HttpResponseHeadersOptionsValidator(resolver);
    }

    private static HttpResponseHeadersOptions CreateOptions(string[] files)
    {
        return new HttpResponseHeadersOptions
        {
            Enabled = true,
            Hosts = new Dictionary<string, HttpResponseHostOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["default"] = new()
                {
                    HostName = "example.com",
                    Files = files
                }
            }
        };
    }

    private string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        _directories.Add(path);
        return path;
    }
}
