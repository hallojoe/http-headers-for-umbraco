using Casko.HttpHeadersForUmbraco.Package.Configuration;
using Casko.HttpHeadersForUmbraco.Package.Http;
using Casko.HttpHeadersForUmbraco.Package.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Casko.HttpHeadersForUmbraco.Tests.Unit;

[TestFixture]
public sealed class DefaultHttpResponseHeaderApplicatorTests
{
    [Test]
    public async Task ApplyAsync_WhenHostAndPathMatch_AppliesHeadersFromFilesInOrder()
    {
        var loader = new FakeHttpResponseHeadersFileLoader(
            new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["headers.http"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Referrer-Policy"] = "strict-origin-when-cross-origin",
                    ["X-Test"] = "first"
                },
                ["headers.override.http"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["X-Test"] = "second"
                }
            });

        var sut = CreateSut(new HttpResponseHeadersOptions
        {
            Enabled = true,
            Hosts = new Dictionary<string, HttpResponseHostOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["default"] = new()
                {
                    HostName = "example.com",
                    Files = ["headers.http", "headers.override.http"]
                }
            }
        }, loader);

        var httpContext = CreateHttpContext("example.com", "/");

        await sut.ApplyAsync(httpContext);

        Assert.Multiple(() =>
        {
            Assert.That(httpContext.Response.Headers["Referrer-Policy"].ToString(), Is.EqualTo("strict-origin-when-cross-origin"));
            Assert.That(httpContext.Response.Headers["X-Test"].ToString(), Is.EqualTo("second"));
        });
    }

    [Test]
    public async Task ApplyAsync_WhenPathDenied_DoesNothing()
    {
        var loader = new FakeHttpResponseHeadersFileLoader();
        var sut = CreateSut(new HttpResponseHeadersOptions
        {
            Enabled = true,
            Hosts = new Dictionary<string, HttpResponseHostOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["default"] = new()
                {
                    HostName = "example.com",
                    DenyPaths = ["/umbraco"],
                    Files = ["headers.http"]
                }
            }
        }, loader);

        var httpContext = CreateHttpContext("example.com", "/umbraco");

        await sut.ApplyAsync(httpContext);

        Assert.That(httpContext.Response.Headers.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task ApplyAsync_WhenHostDoesNotMatch_DoesNothing()
    {
        var loader = new FakeHttpResponseHeadersFileLoader();
        var sut = CreateSut(new HttpResponseHeadersOptions
        {
            Enabled = true,
            Hosts = new Dictionary<string, HttpResponseHostOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["default"] = new()
                {
                    HostName = "example.com",
                    Files = ["headers.http"]
                }
            }
        }, loader);

        var httpContext = CreateHttpContext("other.com", "/");

        await sut.ApplyAsync(httpContext);

        Assert.That(httpContext.Response.Headers.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task ApplyAsync_DoesNotRemoveExistingHeaders()
    {
        var loader = new FakeHttpResponseHeadersFileLoader(
            new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["headers.http"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["X-Test"] = "applied"
                }
            });

        var sut = CreateSut(new HttpResponseHeadersOptions
        {
            Enabled = true,
            Hosts = new Dictionary<string, HttpResponseHostOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["default"] = new()
                {
                    HostName = "example.com",
                    Files = ["headers.http"]
                }
            }
        }, loader);

        var httpContext = CreateHttpContext("example.com", "/");
        httpContext.Response.Headers["Server"] = "Kestrel";

        await sut.ApplyAsync(httpContext);

        Assert.Multiple(() =>
        {
            Assert.That(httpContext.Response.Headers["Server"].ToString(), Is.EqualTo("Kestrel"));
            Assert.That(httpContext.Response.Headers["X-Test"].ToString(), Is.EqualTo("applied"));
        });
    }

    [Test]
    public async Task ApplyAsync_IgnoresLegacyProfilesConfiguration()
    {
        var loader = new FakeHttpResponseHeadersFileLoader();
        var sut = CreateSut(new HttpResponseHeadersOptions
        {
            Enabled = true,
            Hosts = new Dictionary<string, HttpResponseHostOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["default"] = new()
                {
                    HostName = "example.com"
                }
            }
        }, loader);

        var httpContext = CreateHttpContext("example.com", "/");

        await sut.ApplyAsync(httpContext);

        Assert.That(httpContext.Response.Headers.Count, Is.EqualTo(0));
    }

    private static DefaultHttpResponseHeaderApplicator CreateSut(
        HttpResponseHeadersOptions options,
        IHttpResponseHeadersFileLoader loader)
    {
        return new DefaultHttpResponseHeaderApplicator(Options.Create(options), loader);
    }

    private static DefaultHttpContext CreateHttpContext(string host, string path)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Host = new HostString(host);
        httpContext.Request.Path = path;
        return httpContext;
    }

    private sealed class FakeHttpResponseHeadersFileLoader(
        Dictionary<string, IReadOnlyDictionary<string, string>>? fileContents = null) : IHttpResponseHeadersFileLoader
    {
        private readonly Dictionary<string, IReadOnlyDictionary<string, string>> _fileContents =
            fileContents ?? new(StringComparer.OrdinalIgnoreCase);

        public Task<IReadOnlyDictionary<string, string>> LoadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_fileContents.TryGetValue(filePath, out var headers)
                ? headers
                : (IReadOnlyDictionary<string, string>)new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        }
    }
}
