# Http Headers for Umbraco

Http Headers for Umbraco publishes a host-aware HTTP-headers middleware that reads header fragments from files on disk.

## Configuration

```json
{
  "HttpResponseHeaders": {
    "Enabled": true,
    "Hosts": {
      "default": {
        "HostName": "example.com",
        "Files": [ "headers.http" ]
      }
    },
    "XRobotsTag": {
      "Enabled": true,
      "HeaderName": "X-Robots-Tag",
      "HeaderValue": "noindex",
      "NoIndexPropertyAlias": "noIndex",
      "NoIndexPropertyValue": "1",
      "BackgroundJob": {
        "Enabled": true,
        "IntervalSeconds": 3600,
        "RefreshJobDelayInSeconds": 10
      }
    }
  }
}
```

When `XRobotsTag` is enabled, the package periodically queries Examine for content matching `noIndex` and `noIndex_<culture>` and applies the configured header on routed Umbraco content responses.
