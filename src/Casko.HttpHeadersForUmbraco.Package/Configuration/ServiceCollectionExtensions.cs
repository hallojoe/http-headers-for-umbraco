using Casko.HttpHeadersForUmbraco.Package.Http;
using Casko.HttpHeadersForUmbraco.Package.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Casko.HttpHeadersForUmbraco.Package.Configuration;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddHttpResponseHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        var configurationSection = configuration.GetSection(HttpResponseHeadersOptions.Key);
        var options = configurationSection.Get<HttpResponseHeadersOptions>();

        if (options?.Enabled is not true)
        {
            return services;
        }
        
        services
            .AddOptions<HttpResponseHeadersOptions>()
            .Bind(configurationSection)
            .ValidateOnStart();

        services.AddSingleton<IHttpResponseHeadersBindingFileResolver, ContentRootHttpResponseHeadersBindingFileResolver>();
        services.AddSingleton<IHttpResponseHeadersFileParser, DefaultHttpResponseHeadersFileParser>();
        services.AddSingleton<IHttpResponseHeadersFileLoader, DefaultHttpResponseHeadersFileLoader>();
        services.AddSingleton<IValidateOptions<HttpResponseHeadersOptions>>(serviceProvider =>
            new HttpResponseHeadersOptionsValidator(serviceProvider.GetRequiredService<IHttpResponseHeadersBindingFileResolver>()));
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
