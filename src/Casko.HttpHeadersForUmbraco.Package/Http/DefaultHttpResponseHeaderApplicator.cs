using Casko.HttpHeadersForUmbraco.Package.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Casko.HttpHeadersForUmbraco.Package.Http;

/// <inheritdoc />
public sealed class DefaultHttpResponseHeaderApplicator(IOptions<HttpResponseHeadersOptions> options)
    : IHttpResponseHeaderApplicator
{
    private static readonly char[] PathTrimCharacters = [' ', '/'];

    /// <inheritdoc />
    public void Apply(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

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

        foreach (var profileName in hostOptions.Profiles)
        {
            if (!headerOptions.Profiles.TryGetValue(profileName, out var profile) || !profile.Enabled)
            {
                continue;
            }

            ApplyProfile(httpContext.Response.Headers, profile);
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

    private static void ApplyProfile(IHeaderDictionary responseHeaders, HttpResponseHeaderProfileOptions profile)
    {
        foreach (var (headerName, values) in profile.Add)
        {
            responseHeaders[headerName] = new StringValues(string.Join("", values));
        }
        
        foreach (var headerName in profile.Remove)
        {
            responseHeaders.Remove(headerName);
        }
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
}
