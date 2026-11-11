using Casko.HttpHeadersForUmbraco.Package.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;

namespace Casko.HttpHeadersForUmbraco.Package.Services;

public sealed class PublishedRequestHandler(
    IOptions<HttpResponseHeadersOptions> options,
    IPublishedUrlProvider publishedUrlProvider) : INotificationHandler<RoutingRequestNotification>
{
    public void Handle(RoutingRequestNotification notification)
    {
        var settings = options.Value;
        if (!settings.Enabled || (!settings.CanonicalLink.Enabled && !settings.XRobotsTag.Enabled))
        {
            return;
        }

        var requestBuilder = notification.RequestBuilder;
        var content = requestBuilder.PublishedContent;
        if (content is null)
        {
            return;
        }

        var existingHeaders = requestBuilder.Build().Headers;
        var headers = existingHeaders is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(existingHeaders, StringComparer.OrdinalIgnoreCase);

        if (settings.CanonicalLink.Enabled)
        {
            var url = publishedUrlProvider.GetUrl(content, UrlMode.Absolute, requestBuilder.Culture, requestBuilder.Uri);
            if (Uri.TryCreate(url, UriKind.Absolute, out var canonicalUri) &&
                (canonicalUri.Scheme == Uri.UriSchemeHttp || canonicalUri.Scheme == Uri.UriSchemeHttps))
            {
                headers["Link"] = $"<{canonicalUri.AbsoluteUri}>; rel=\"canonical\"";
            }
        }

        if (settings.XRobotsTag.Enabled)
        {
            var noIndex = content.GetProperty(settings.XRobotsTag.NoIndexPropertyAlias)
                ?.GetValue(requestBuilder.Culture, requestBuilder.Segment);

            if (MatchesPropertyValue(noIndex, settings.XRobotsTag.NoIndexPropertyValue))
            {
                headers[settings.XRobotsTag.HeaderName] = settings.XRobotsTag.HeaderValue;
            }
        }

        if (headers.Count > 0)
        {
            requestBuilder.SetHeaders(headers);
        }
    }

    private static bool MatchesPropertyValue(object? value, string expectedValue)
    {
        if (value is bool booleanValue)
        {
            return expectedValue switch
            {
                "1" => booleanValue,
                "0" => !booleanValue,
                _ => bool.TryParse(expectedValue, out var expectedBoolean) && booleanValue == expectedBoolean
            };
        }

        return value is not null && string.Equals(value.ToString(), expectedValue, StringComparison.Ordinal);
    }
}
