namespace Casko.HttpHeadersForUmbraco.Package.Configuration;

/// <summary>
/// Configuration for HTTP response headers.
/// </summary>
public sealed class HttpResponseHeadersOptions
{
    public const string Key = "HttpResponseHeaders";

    public bool Enabled { get; init; }

    public Dictionary<string, HttpResponseHostOptions> Hosts { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, HttpResponseHeaderProfileOptions> Profiles { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Configuration for a host.
/// </summary>
public sealed class HttpResponseHostOptions
{
    public required string HostName { get; init; }

    public string[] AllowPaths { get; init; } = [];

    public string[] DenyPaths { get; init; } = [];

    public string[] Profiles { get; init; } = [];
}

/// <summary>
/// Configuration for a header profile.
/// </summary>
public sealed class HttpResponseHeaderProfileOptions
{
    public bool Enabled { get; init; }

    public string[] Remove { get; init; } = [];

    /// <summary>
    /// Headers to add. Values may be either a string or a string[] (e.g. Content-Security-Policy).
    /// </summary>
    public Dictionary<string, string[]> Add { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
