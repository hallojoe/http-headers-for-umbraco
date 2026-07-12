using Casko.HttpHeadersForUmbraco.Package.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Casko.HttpHeadersForUmbraco.Package.Services;

public sealed class PublishedRequestHandler(IOptions<HttpResponseHeadersOptions> options) : INotificationHandler<RoutingRequestNotification>
{
    public void Handle(RoutingRequestNotification notification)
    {
        if (options.Value.CanonicalLink.Enabled is not true || options.Value.XRobotsTag.Enabled is not true)
        {
            return;
        }

        var headers = new Dictionary<string, string>();
        var requestBuilder = notification.RequestBuilder;
        var content = requestBuilder.PublishedContent;

        if (options.Value.CanonicalLink.Enabled)
        {
            var url = content?.Url(requestBuilder.Culture, UrlMode.Absolute);

            headers.Add("Link", $"<{url}>; rel=\"canonical\"");
        }

        if (options.Value.XRobotsTag.Enabled)
        {
            var noIndex = content?.Value<string>(options.Value.XRobotsTag.NoIndexPropertyAlias);
            var noIndexValue = options.Value.XRobotsTag.NoIndexPropertyValue;

            if (noIndex?.Equals(noIndexValue) is true)
            {
                headers.Add(options.Value.XRobotsTag.HeaderName, options.Value.XRobotsTag.HeaderValue);
            }
        }

        if (headers.Count > 0)
        {
            requestBuilder.SetHeaders(headers);
        }
    }
}
