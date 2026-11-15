namespace Casko.HttpHeadersForUmbraco.Package.Services;

public interface IHttpResponseHeadersFileLoader
{
    Task<IReadOnlyDictionary<string, string>> LoadAsync(string filePath, CancellationToken cancellationToken = default);
}
