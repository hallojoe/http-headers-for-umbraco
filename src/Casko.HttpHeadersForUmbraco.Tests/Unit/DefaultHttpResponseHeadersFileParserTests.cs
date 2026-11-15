using Casko.HttpHeadersForUmbraco.Package.Services;

namespace Casko.HttpHeadersForUmbraco.Tests.Unit;

[TestFixture]
public sealed class DefaultHttpResponseHeadersFileParserTests
{
    [Test]
    public void Parse_WhenSingleLineHeaders_ReturnsHeaders()
    {
        var sut = new DefaultHttpResponseHeadersFileParser();

        var result = sut.Parse("""
            X-Frame-Options: SAMEORIGIN
            Referrer-Policy: strict-origin-when-cross-origin
            """);

        Assert.Multiple(() =>
        {
            Assert.That(result["X-Frame-Options"], Is.EqualTo("SAMEORIGIN"));
            Assert.That(result["Referrer-Policy"], Is.EqualTo("strict-origin-when-cross-origin"));
        });
    }

    [Test]
    public void Parse_WhenContinuationLinesExist_JoinsWithSpaces()
    {
        var sut = new DefaultHttpResponseHeadersFileParser();

        var result = sut.Parse("""
            Content-Security-Policy:
                default-src 'self';
                script-src
                'self'
                https://cdn.jsdelivr.net;
            """);

        Assert.That(result["Content-Security-Policy"], Is.EqualTo("default-src 'self'; script-src 'self' https://cdn.jsdelivr.net;"));
    }

    [Test]
    public void Parse_IgnoresBlankLines()
    {
        var sut = new DefaultHttpResponseHeadersFileParser();

        var result = sut.Parse("""
            X-Test: a

            X-Other: b
            """);

        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public void Parse_WhenHeaderRepeated_LastValueWins()
    {
        var sut = new DefaultHttpResponseHeadersFileParser();

        var result = sut.Parse("""
            X-Test: first
            X-Test: second
            """);

        Assert.That(result["X-Test"], Is.EqualTo("second"));
    }
}
