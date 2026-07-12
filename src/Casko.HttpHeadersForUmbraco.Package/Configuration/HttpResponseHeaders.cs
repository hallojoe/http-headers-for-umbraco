namespace Casko.HttpHeadersForUmbraco.Package.Configuration;

/// <summary>
/// Configuration for HTTP response headers.
/// </summary>
public sealed class HttpResponseHeadersOptions
{
    public const string Key = "HttpResponseHeaders";

    public bool Enabled { get; init; }

    public Dictionary<string, HttpResponseHostOptions> Hosts { get; init; } = new(StringComparer.OrdinalIgnoreCase);
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
