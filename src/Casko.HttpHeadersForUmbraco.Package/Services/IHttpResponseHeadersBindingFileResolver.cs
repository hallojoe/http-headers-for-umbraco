namespace Casko.HttpHeadersForUmbraco.Package.Services;

public interface IHttpResponseHeadersBindingFileResolver
{
    string ResolvePath(string includePath);

    bool Exists(string includePath);

    Task<string> ReadAsync(string includePath, CancellationToken cancellationToken = default);
}
