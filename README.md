# HTTP Headers for Umbraco

Developer and agent guide for `Casko.HttpHeadersForUmbraco`.

Read the [product overview](.github/README.md) for scope and acceptance criteria.
Read the [package guide](docs/README.md) for installation and configuration.

## Requirements

- .NET 10 SDK.
- Umbraco CMS 17.8.1 or later within major version 17.
- Windows or Linux for the CI checks.

Central dependencies use `[17.8.1,18.0.0)` in `src/Directory.Packages.props`.
Restore selects the lowest available version in that range, currently 17.8.1.
Consumers can use later 17.x versions without changing this package.
Umbraco 18 requires separate compatibility work.

## Repository layout

| Path | Purpose |
| --- | --- |
| `src/Casko.HttpHeadersForUmbraco.Package` | Package source and Umbraco integration |
| `src/Casko.HttpHeadersForUmbraco.Tests` | NUnit regression tests |
| `src/Casko.HttpHeadersForUmbraco.TestSite` | Optional manual demonstration site |
| `src/Casko.HttpHeadersForUmbraco.slnx` | Full solution, including the demonstration site |
| `docs/README.md` | NuGet README and configuration reference |
| `.github/README.md` | Product overview for product managers |
| `.github/workflows` | Validation and release workflows |

## Build and validate

Run these commands from the repository root.
They build the package and tests without building the demonstration site.

```sh
dotnet restore src/Casko.HttpHeadersForUmbraco.Tests/Casko.HttpHeadersForUmbraco.Tests.csproj
dotnet test src/Casko.HttpHeadersForUmbraco.Tests/Casko.HttpHeadersForUmbraco.Tests.csproj --configuration Release --no-restore
dotnet pack src/Casko.HttpHeadersForUmbraco.Package/Casko.HttpHeadersForUmbraco.Package.csproj --configuration Release --no-build --no-restore --output ./artifacts
```

CI runs these checks on Ubuntu and Windows.
The case-sensitive filesystem regression test skips filesystems that ignore case.
Release publishing also requires the tests to pass.

The NuGet archive must contain `README.md` from `docs/README.md`.
Its Umbraco dependency must retain the `[17.8.1,18.0.0)` range.

## Runtime behavior

`HttpHeadersComposer` registers configuration, services, middleware, and the routing notification handler.
`HttpResponseHeaders.Enabled` controls the package.

The file-header path has four stages:

1. Match `Request.Host.Value` against a configured `HostName`, ignoring case.
2. Apply path filters. Denied paths take priority over allowed paths.
3. Read and parse each configured file, in order.
4. Write response headers in `OnStarting`. Later values replace earlier values with the same name.

Host matching includes the port when the request supplies one.
Paths match complete segments: `/news` matches `/news/article`, but not `/newsletter`.
An empty allow list allows all paths. A configured `/` matches only the root path.
Filters apply to file headers on requests that reach the middleware.

Files are read for each matching response. Parsed headers are not cached.
Startup validation checks configured paths and file existence.
The resolver rejects paths outside the content root through a normalized path comparison.
That comparison ignores case on Windows and respects case on Linux.
Configuration and header files are trusted deployment inputs; do not use symbolic links to external files.

`PublishedRequestHandler` applies content headers to routed Umbraco content.
These headers use their own switches, independent of each other and of file-header host filters.

- Canonical links use the URL provider, request culture, and current request URI.
- Missing content and invalid absolute HTTP or HTTPS URLs produce no canonical header.
- Robots rules read the published property's value for the request culture and segment.
- Boolean properties accept configured values `1`, `0`, `true`, and `false`.
- Other property values use exact string comparison.
- Existing routing headers remain unless a generated header replaces the same name, ignoring case.
- File headers can override content headers because they run when the response starts.

There is no package-specific Examine service, index scan, background job, or search cache.
Umbraco itself still has Examine dependencies.

## Working rules

- Keep package work separate from the optional demonstration site.
- Add regression tests for changed header behavior and path handling.
- Use portable filesystem paths in tests and remove temporary test directories.
- Update the package guide when configuration or runtime behavior changes.
- Update the product overview when supported versions or user-visible behavior changes.
- Do not edit generated `bin` or `obj` files.
- Run tests and package validation before committing.

Development uses `develop`. Pull requests promote changes into `main`.
The release workflow publishes packages when a GitHub release is published.
Stable release tags use `vX.Y.Z` and target `main`.
Prerelease tags use a suffix, such as `v1.0.0-rc.1`, and target a non-main branch.
