namespace Casko.HttpHeadersForUmbraco.Package.Services;

public interface IHttpResponseHeadersFileParser
{
    IReadOnlyDictionary<string, string> Parse(string document);
}
