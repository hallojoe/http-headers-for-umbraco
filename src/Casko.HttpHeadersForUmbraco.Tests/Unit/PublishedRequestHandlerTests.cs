using Casko.HttpHeadersForUmbraco.Package.Configuration;
using Casko.HttpHeadersForUmbraco.Package.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;

namespace Casko.HttpHeadersForUmbraco.Tests.Unit;

[TestFixture]
public sealed class PublishedRequestHandlerTests
{
    private IPublishedContent _content = null!;
    private IPublishedProperty _noIndex = null!;
    private IPublishedUrlProvider _urlProvider = null!;
    private PublishedRequestBuilder _request = null!;

    [SetUp]
    public void SetUp()
    {
        _content = Substitute.For<IPublishedContent>();
        _noIndex = Substitute.For<IPublishedProperty>();
        _content.GetProperty("noIndex").Returns(_noIndex);
        _noIndex.GetValue("en-US", null).Returns(true);
        _urlProvider = Substitute.For<IPublishedUrlProvider>();
        _request = new PublishedRequestBuilder(new Uri("https://example.com/page"), Substitute.For<IFileService>());
        _request.SetPublishedContent(_content);
        _request.SetCulture("en-US");
        _urlProvider.GetUrl(_content, UrlMode.Absolute, "en-US", _request.Uri).Returns("https://example.com/page");
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    public void Handle_FeaturesWorkIndependently(bool canonical, bool robots)
    {
        Handle(canonical, robots);

        var headers = _request.Build().Headers;
        Assert.Multiple(() =>
        {
            Assert.That(headers?.ContainsKey("Link") ?? false, Is.EqualTo(canonical));
            Assert.That(headers?.ContainsKey("X-Robots-Tag") ?? false, Is.EqualTo(robots));
        });
        if (canonical)
        {
            Assert.That(headers!["Link"], Is.EqualTo("<https://example.com/page>; rel=\"canonical\""));
        }
        if (robots)
        {
            Assert.That(headers!["X-Robots-Tag"], Is.EqualTo("noindex,nofollow"));
        }
    }

    [Test]
    public void Handle_WhenPackageDisabled_PreservesHeaders()
    {
        _request.SetHeaders(new Dictionary<string, string> { ["X-Existing"] = "keep" });

        Handle(canonical: true, robots: true, enabled: false);

        Assert.That(_request.Build().Headers, Is.EquivalentTo(new Dictionary<string, string> { ["X-Existing"] = "keep" }));
        _urlProvider.DidNotReceiveWithAnyArgs().GetUrl(default(IPublishedContent)!, default, default, default);
    }

    [TestCase(true, "1", true)]
    [TestCase(false, "1", false)]
    [TestCase(true, "true", true)]
    [TestCase(false, "true", false)]
    [TestCase(true, "TRUE", true)]
    [TestCase(false, "0", true)]
    [TestCase(false, "false", true)]
    [TestCase(true, "0", false)]
    [TestCase(true, "yes", false)]
    [TestCase("hide", "hide", true)]
    [TestCase("show", "hide", false)]
    [TestCase("Hide", "hide", false)]
    [TestCase("1", "1", true)]
    [TestCase(null, "1", false)]
    public void Handle_MatchesPublishedPropertyValue(object? value, string expectedValue, bool matches)
    {
        _noIndex.GetValue("en-US", null).Returns(value);

        Handle(canonical: false, robots: true, expectedValue: expectedValue);

        Assert.That(_request.Build().Headers?.ContainsKey("X-Robots-Tag") ?? false, Is.EqualTo(matches));
    }

    [Test]
    public void Handle_UsesRequestCultureAndSegment()
    {
        _request.SetCulture("da-DK");
        _request.SetSegment("campaign");
        _noIndex.GetValue("da-DK", "campaign").Returns(true);

        Handle(canonical: false, robots: true);

        Assert.That(_request.Build().Headers!["X-Robots-Tag"], Is.EqualTo("noindex,nofollow"));
        _noIndex.Received(1).GetValue("da-DK", "campaign");
    }

    [Test]
    public void Handle_PreservesExistingHeadersAndReplacesConfiguredHeaderIgnoringCase()
    {
        _request.SetHeaders(new Dictionary<string, string>
        {
            ["X-Existing"] = "keep",
            ["x-robots-tag"] = "old"
        });

        Handle(canonical: true, robots: true);

        var headers = _request.Build().Headers!;
        Assert.Multiple(() =>
        {
            Assert.That(headers["X-Existing"], Is.EqualTo("keep"));
            Assert.That(headers["X-Robots-Tag"], Is.EqualTo("noindex,nofollow"));
            Assert.That(headers.Count, Is.EqualTo(3));
        });
    }

    [Test]
    public void Handle_WhenContentMissing_PreservesHeadersWithoutAddingCanonicalLink()
    {
        _request.SetPublishedContent(null);
        _request.SetHeaders(new Dictionary<string, string> { ["X-Existing"] = "keep" });

        Handle(canonical: true, robots: true);

        Assert.That(_request.Build().Headers, Is.EquivalentTo(new Dictionary<string, string> { ["X-Existing"] = "keep" }));
        _urlProvider.DidNotReceiveWithAnyArgs().GetUrl(default(IPublishedContent)!, default, default, default);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("#")]
    [TestCase("/relative")]
    [TestCase("ftp://example.com/page")]
    public void Handle_WhenCanonicalUrlInvalid_PreservesExistingLink(string? url)
    {
        _urlProvider.GetUrl(_content, UrlMode.Absolute, "en-US", _request.Uri).Returns(url!);
        _request.SetHeaders(new Dictionary<string, string> { ["Link"] = "</styles.css>; rel=\"preload\"" });

        Handle(canonical: true, robots: false);

        Assert.That(_request.Build().Headers!["Link"], Is.EqualTo("</styles.css>; rel=\"preload\""));
    }

    private void Handle(bool canonical, bool robots, bool enabled = true, string expectedValue = "1")
    {
        var settings = new HttpResponseHeadersOptions
        {
            Enabled = enabled,
            CanonicalLink = new() { Enabled = canonical },
            XRobotsTag = new() { Enabled = robots, NoIndexPropertyValue = expectedValue }
        };
        var handler = new PublishedRequestHandler(Options.Create(settings), _urlProvider);
        handler.Handle(new RoutingRequestNotification(_request));
    }
}
