# HTTP Headers for Umbraco

HTTP Headers for Umbraco gives teams consistent control over response headers across Umbraco websites.
It supports security policies, search indexing rules, and canonical URLs.

## Product scope

| Capability | Outcome |
| --- | --- |
| Header files | Developers maintain response-header policies in deployment files. |
| Host and path rules | Different domains and URL sections can receive different file-based policies. |
| Search indexing rules | A content property determines when the response receives an `X-Robots-Tag` header. |
| Canonical links | Routed content can identify its preferred URL through an HTTP `Link` header. |
| Independent switches | Teams can enable indexing rules, canonical links, or both. |

## Supported platform

The package requires .NET 10 and Umbraco CMS 17.8.1 or later within the 17.x release line.
Umbraco 18 support is outside the current scope.
Automated validation covers Ubuntu and Windows.

## Expected behavior

- Disabled features add no headers.
- A checked Boolean indexing property can add the configured robots directive.
- Canonical links require published content and a valid absolute web URL.
- Existing headers remain unless the configured feature replaces the same header.
- File-based rules can override content-generated headers.
- Header files can be updated without rebuilding the package.

The package reads published content during routing.
It does not scan a search index or run a background indexing job.

## Delivery and acceptance

Developers install the package and configure the required headers and content property.
Content editors can then control indexing through that property.
Header files and host rules remain developer-managed configuration.

Before delivery, confirm:

- The selected header appears on the intended responses.
- File policies match the intended hosts and paths.
- Indexing and canonical features work independently.
- Unrelated response headers remain present.
- Automated tests and NuGet packaging pass.

See the [package guide](../docs/README.md) for configuration.
See the [developer and agent guide](../README.md) for implementation and validation.
