using Casko.HttpHeadersForUmbraco.Package.Services;
using Microsoft.Extensions.Options;

namespace Casko.HttpHeadersForUmbraco.Package.Configuration;

public sealed class HttpResponseHeadersOptionsValidator(
    IHttpResponseHeadersBindingFileResolver bindingFileResolver) : IValidateOptions<HttpResponseHeadersOptions>
{
    public ValidateOptionsResult Validate(string? name, HttpResponseHeadersOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        foreach (var hostEntry in options.Hosts)
        {
            var hostKey = hostEntry.Key;
            var host = hostEntry.Value;

            if (host is null)
            {
                failures.Add($"HttpResponseHeaders:Hosts:{hostKey} must not be null.");
                continue;
            }

            foreach (var file in host.Files)
            {
                if (string.IsNullOrWhiteSpace(file))
                {
                    failures.Add($"HttpResponseHeaders:Hosts:{hostKey}:Files contains an empty file reference.");
                    continue;
                }

                try
                {
                    var fullPath = bindingFileResolver.ResolvePath(file);
                    if (!File.Exists(fullPath))
                    {
                        failures.Add($"HttpResponseHeaders:Hosts:{hostKey}:Files references missing file '{file.Trim()}'.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    failures.Add(ex.Message);
                }
            }
        }

        if (options.XRobotsTag.Enabled)
        {
            if (string.IsNullOrWhiteSpace(options.XRobotsTag.HeaderName))
            {
                failures.Add("HttpResponseHeaders:XRobotsTag:HeaderName must not be empty.");
            }

            if (string.IsNullOrWhiteSpace(options.XRobotsTag.HeaderValue))
            {
                failures.Add("HttpResponseHeaders:XRobotsTag:HeaderValue must not be empty.");
            }

            if (string.IsNullOrWhiteSpace(options.XRobotsTag.NoIndexPropertyAlias))
            {
                failures.Add("HttpResponseHeaders:XRobotsTag:NoIndexPropertyAlias must not be empty.");
            }

            if (string.IsNullOrWhiteSpace(options.XRobotsTag.NoIndexPropertyValue))
            {
                failures.Add("HttpResponseHeaders:XRobotsTag:NoIndexPropertyValue must not be empty.");
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
