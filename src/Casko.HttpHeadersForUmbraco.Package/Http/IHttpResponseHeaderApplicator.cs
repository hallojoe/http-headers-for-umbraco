using Microsoft.AspNetCore.Http;

namespace Casko.HttpHeadersForUmbraco.Package.Http;

/// <summary>
/// Applies configured HTTP response header mutations for the current request.
/// </summary>
public interface IHttpResponseHeaderApplicator
{
    /// <summary>
    /// Applies configured header changes to the current response.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    void Apply(HttpContext httpContext);
}
