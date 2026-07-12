using Casko.HttpHeadersForUmbraco.Package.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Casko.HttpHeadersForUmbraco.Package.Services;

public sealed class PublishedRequestXRobotsTagHandler(IOptions<HttpResponseHeadersOptions> options) : INotificationHandler<RoutingRequestNotification>
{
    public void Handle(RoutingRequestNotification notification)
    {
        var requestBuilder = notification.RequestBuilder;
        var content = requestBuilder.PublishedContent;

        var noIndex = content?.Value<string>(options.Value.XRobotsTag.NoIndexPropertyAlias);
        var noIndexValue = options.Value.XRobotsTag.NoIndexPropertyValue;
        if (noIndex?.Equals(noIndexValue) is true)
        {
            requestBuilder.SetHeaders(new Dictionary<string, string>
            {
                { options.Value.XRobotsTag.HeaderName, options.Value.XRobotsTag.HeaderValue }
            });
        }
    }
}
