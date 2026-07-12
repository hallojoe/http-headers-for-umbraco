using Casko.HttpHeadersForUmbraco.Package.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Casko.HttpHeadersForUmbraco.Package.Configuration;

public sealed class HttpHeadersComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddHttpResponseHeaders(builder.Config);

        var configurationSection = builder.Config.GetSection(HttpResponseHeadersOptions.Key);

        var options = configurationSection.Get<HttpResponseHeadersOptions>();
        if (options is { Enabled: true, XRobotsTag.Enabled: true })
        {
            builder.AddNotificationHandler<RoutingRequestNotification, PublishedRequestXRobotsTagHandler>();
        }
        if (options is { Enabled: true, CanonicalLink.Enabled: true })
        {
            builder.AddNotificationHandler<RoutingRequestNotification, PublishedRequestCanonicalLinkHandler>();
        }

        
    }
}
