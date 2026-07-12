using Casko.HttpHeadersForUmbraco.Package.Configuration;
using Casko.HttpHeadersForUmbraco.Package.Services;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Casko.HttpHeadersForUmbraco.Tests.Unit;

[TestFixture]
public sealed class HttpResponseHeadersOptionsValidatorTests
{
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
        var sut = CreateSut(CreateTempDirectory());

        var result = sut.Validate(null, CreateOptions(["..\\outside.http"]));

        Assert.That(result.Failed, Is.True);
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

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
