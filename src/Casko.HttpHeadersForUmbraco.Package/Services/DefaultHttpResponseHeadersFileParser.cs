namespace Casko.HttpHeadersForUmbraco.Package.Services;

public sealed class DefaultHttpResponseHeadersFileParser : IHttpResponseHeadersFileParser
{
    public IReadOnlyDictionary<string, string> Parse(string document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? currentHeaderName = null;
        List<string>? currentSegments = null;

        foreach (var rawLine in EnumerateLines(document))
        {
            if (string.IsNullOrWhiteSpace(rawLine))
            {
                continue;
            }

            if (char.IsWhiteSpace(rawLine[0]))
            {
                if (currentHeaderName is null || currentSegments is null)
                {
                    throw new InvalidOperationException($"Invalid header continuation line '{rawLine}'.");
                }

                var continuation = rawLine.Trim();
                if (continuation.Length > 0)
                {
                    currentSegments.Add(continuation);
                }

                headers[currentHeaderName] = string.Join(" ", currentSegments);
                continue;
            }

            var colonIndex = rawLine.IndexOf(':');
            if (colonIndex <= 0)
            {
                throw new InvalidOperationException($"Invalid header line '{rawLine}'.");
            }

            currentHeaderName = rawLine[..colonIndex].Trim();
            if (currentHeaderName.Length == 0)
            {
                throw new InvalidOperationException($"Invalid header line '{rawLine}'.");
            }

            currentSegments = [];

            var value = rawLine[(colonIndex + 1)..].Trim();
            if (value.Length > 0)
            {
                currentSegments.Add(value);
            }

            headers[currentHeaderName] = string.Join(" ", currentSegments);
        }

        return headers;
    }

    private static IEnumerable<string> EnumerateLines(string document)
    {
        using var reader = new StringReader(document);
        while (reader.ReadLine() is { } line)
        {
            yield return line;
        }
    }
}
