using Casko.HttpHeadersForUmbraco.Package.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Casko.HttpHeadersForUmbraco.Package.Services;

public sealed class PublishedRequestCanonicalLinkHandler(IOptions<HttpResponseHeadersOptions> options) : INotificationHandler<RoutingRequestNotification>
{
    public void Handle(RoutingRequestNotification notification)
    {
        if (options.Value.CanonicalLink.Enabled is not true)
        {
            return;
        }

        var requestBuilder = notification.RequestBuilder;
        var content = requestBuilder.PublishedContent;
        var url = content?.Url(requestBuilder.Culture, UrlMode.Absolute);
        requestBuilder.SetHeaders(new Dictionary<string, string>
        {
            { "Link", $"<{url}>; rel=\"canonical\"" }
        });
    }
}
