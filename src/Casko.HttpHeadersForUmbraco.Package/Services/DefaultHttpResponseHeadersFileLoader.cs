namespace Casko.HttpHeadersForUmbraco.Package.Services;

public sealed class DefaultHttpResponseHeadersFileLoader(
    IHttpResponseHeadersBindingFileResolver bindingFileResolver,
    IHttpResponseHeadersFileParser fileParser) : IHttpResponseHeadersFileLoader
{
    public async Task<IReadOnlyDictionary<string, string>> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fileContents = await bindingFileResolver.ReadAsync(filePath, cancellationToken);
        return fileParser.Parse(fileContents);
    }
}
