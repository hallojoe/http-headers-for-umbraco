using Casko.HttpHeadersForUmbraco.Package.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Casko.HttpHeadersForUmbraco.Package.Configuration;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddHttpResponseHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        var configurationSection = configuration.GetSection(HttpResponseHeadersOptions.Key);
        
        services
            .AddOptions<HttpResponseHeadersOptions>()
            .Bind(configurationSection);

        services.AddSingleton<IHttpResponseHeaderApplicator, DefaultHttpResponseHeaderApplicator>();
        
        services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter(
                nameof(HttpResponseHeadersMiddleware),
                postPipeline: app =>
                {
                    app.UseMiddleware<HttpResponseHeadersMiddleware>();
                }));
        });

        return services;
    }
}
