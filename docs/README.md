# HTTP Headers for Umbraco

Configure HTTP response headers from files, content indexing rules, and canonical URLs.

## Requirements and installation

Requires .NET 10 and Umbraco CMS 17.8.1 or later within major version 17.
The package dependency range is `[17.8.1,18.0.0)`.

```sh
dotnet add package Casko.HttpHeadersForUmbraco
```

Umbraco discovers the package composer automatically.
Add configuration to the application's `appsettings.json`.

## Configuration

```json
{
  "HttpResponseHeaders": {
    "Enabled": true,
    "Hosts": {
      "default": {
        "HostName": "example.com",
        "AllowPaths": [],
        "DenyPaths": [ "/private" ],
        "Files": [ "headers.http" ]
      }
    },
    "XRobotsTag": {
      "Enabled": true,
      "HeaderName": "X-Robots-Tag",
      "HeaderValue": "noindex",
      "NoIndexPropertyAlias": "noIndex",
      "NoIndexPropertyValue": "1"
    },
    "CanonicalLink": {
      "Enabled": false
    }
  }
}
```

Set `HttpResponseHeaders.Enabled` to `false` to disable the package.
`XRobotsTag.Enabled` and `CanonicalLink.Enabled` work independently.

## Header files

Create `headers.http` in the application's content root:

```http
X-Content-Type-Options: nosniff
Referrer-Policy: strict-origin-when-cross-origin
```

Use one `Header-Name: value` entry per line.
Indent continuation lines to combine them with the previous header value.
Blank lines are ignored. Comments are not supported.
Repeated header names use the last value, ignoring case.
Files are processed in configuration order, and later files can replace earlier values.

Paths resolve relative to the content root and must remain within it.
Include header files in the application's deployment output.
Missing files prevent startup when the package is enabled.
Files are read for each matching response, so changes need no package rebuild.

## Host and path rules

`HostName` must match the request host, including its port when present.
Matching ignores case. Host wildcards are not supported.
Each key under `Hosts` is a configuration label; `default` is not a fallback host.

An empty `AllowPaths` list permits all paths.
`DenyPaths` takes priority.
`/news` matches `/news` and `/news/article`, but not `/newsletter`.
`/` matches only the root path.

These rules control file headers on requests that reach the middleware.
Content-generated robots and canonical headers use their own switches for routed Umbraco content.

## Search indexing rules

Create a published content property with the alias `noIndex`, or configure another alias.
For a Boolean property, the default `NoIndexPropertyValue` of `"1"` matches a checked value.
Boolean properties also support `"0"`, `"true"`, and `"false"`.
Other property types use exact string comparison against the configured value.

The handler reads the published property for the request culture and segment.
When it matches, the handler writes `HeaderName` with `HeaderValue`.
The default header value is `noindex,nofollow`; the example changes it to `noindex`.

There are no background queries or package-specific Examine services.

## Canonical links and precedence

Enable `CanonicalLink.Enabled` to add an absolute canonical URL from Umbraco's URL provider.
The handler skips missing content and invalid URLs.

Existing routing headers remain unless a generated header replaces the same name.
Configured file headers run when the response starts and can override content-generated headers.
Avoid defining the same header in both places unless that override is intended.

See the [repository developer guide](https://github.com/hallojoe/http-headers-for-umbraco/blob/main/README.md) for source and validation commands.
