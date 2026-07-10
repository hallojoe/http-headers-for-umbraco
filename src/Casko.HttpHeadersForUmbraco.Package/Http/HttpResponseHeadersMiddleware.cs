using Microsoft.AspNetCore.Http;

namespace Casko.HttpHeadersForUmbraco.Package.Http;

public sealed class HttpResponseHeadersMiddleware(
    RequestDelegate next,
    IHttpResponseHeaderApplicator httpResponseHeaderApplicator)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(static state =>
        {
            var (applicator, httpContext) = ((IHttpResponseHeaderApplicator Applicator, HttpContext HttpContext))state;
            applicator.Apply(httpContext);
            return Task.CompletedTask;
        }, (httpResponseHeaderApplicator, context));

        await next(context);
    }
}
