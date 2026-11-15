namespace Casko.HttpHeadersForUmbraco.Package.Configuration;

/// <summary>
/// Configuration for HTTP response headers.
/// </summary>
public sealed class HttpResponseHeadersOptions
{
    public const string Key = "HttpResponseHeaders";

    public bool Enabled { get; init; }

    public Dictionary<string, HttpResponseHostOptions> Hosts { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public HttpResponseHeadersXRobotsTagOptions XRobotsTag { get; init; } = new();
    public HttpResponseHeadersCanonicalLinkOptions CanonicalLink { get; init; } = new();
}

public sealed class HttpResponseHeadersCanonicalLinkOptions
{
    public bool Enabled { get; init; }
    public string? PropertyAlias { get; init; }
}

public sealed class HttpResponseHeadersXRobotsTagOptions
{
    public bool Enabled { get; init; }

    public string HeaderName { get; init; } = "X-Robots-Tag";

    public string HeaderValue { get; init; } = "noindex,nofollow";

    public string NoIndexPropertyAlias { get; init; } = "noIndex";

    public string NoIndexPropertyValue { get; init; } = "1";

}

/// <summary>
/// Configuration for a host.   
/// </summary>
public sealed class HttpResponseHostOptions
{
    public required string HostName { get; init; }

    public string[] AllowPaths { get; init; } = [];

    public string[] DenyPaths { get; init; } = [];

    public string[] Files { get; init; } = [];
}
