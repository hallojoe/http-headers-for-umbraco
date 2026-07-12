using Casko.HttpHeadersForUmbraco.Package.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Casko.HttpHeadersForUmbraco.Package.Services;

namespace Casko.HttpHeadersForUmbraco.Package.Http;

/// <inheritdoc />
public sealed class DefaultHttpResponseHeaderApplicator(
    IOptions<HttpResponseHeadersOptions> options,
    IHttpResponseHeadersFileLoader fileLoader)
    : IHttpResponseHeaderApplicator
{
    private static readonly char[] PathTrimCharacters = [' ', '/'];

    /// <inheritdoc />
    public async Task ApplyAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        cancellationToken.ThrowIfCancellationRequested();

        var headerOptions = options.Value;
        if (!headerOptions.Enabled)
        {
            return;
        }

        var hostOptions = ResolveHostOptions(headerOptions, httpContext.Request.Host.Value ?? string.Empty);
        if (hostOptions is null || !IsPathAllowed(hostOptions, httpContext.Request.Path))
        {
            return;
        }

        foreach (var filePath in Clean(hostOptions.Files))
        {
            var headers = await fileLoader.LoadAsync(filePath, cancellationToken);
            foreach (var (headerName, value) in headers)
            {
                httpContext.Response.Headers[headerName] = new StringValues(value);
            }
        }
    }

    private static HttpResponseHostOptions? ResolveHostOptions(HttpResponseHeadersOptions options, string requestHost)
    {
        if (string.IsNullOrWhiteSpace(requestHost))
        {
            return null;
        }

        foreach (var hostOptions in options.Hosts.Values)
        {
            if (string.Equals(hostOptions.HostName, requestHost, StringComparison.OrdinalIgnoreCase))
            {
                return hostOptions;
            }
        }

        return null;
    }

    private static bool IsPathAllowed(HttpResponseHostOptions hostOptions, PathString requestPath)
    {
        var path = NormalizePath(requestPath.Value);
        var hasAllowPaths = hostOptions.AllowPaths.Length > 0;
        var isAllowed = !hasAllowPaths || hostOptions.AllowPaths.Any(allowPath => PathMatches(path, allowPath));
        if (!isAllowed)
        {
            return false;
        }

        return !hostOptions.DenyPaths.Any(denyPath => PathMatches(path, denyPath));
    }

    private static bool PathMatches(string requestPath, string configuredPath)
    {
        var normalizedConfiguredPath = NormalizePath(configuredPath);
        if (normalizedConfiguredPath.Length == 0)
        {
            return requestPath.Length == 0;
        }

        if (!requestPath.StartsWith(normalizedConfiguredPath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return requestPath.Length == normalizedConfiguredPath.Length ||
               requestPath[normalizedConfiguredPath.Length] == '/';
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path == "/")
        {
            return string.Empty;
        }

        return path.Trim(PathTrimCharacters);
    }

    private static IEnumerable<string> Clean(IEnumerable<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim());
    }
}
