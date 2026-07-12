using Examine;
using Examine.Search;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Examine;

namespace Casko.HttpHeadersForUmbraco.Package.Services;

public interface IExamineSearchService
{
    /// <summary>
    /// Executes a raw query against the configured Examine index.
    /// </summary>
    /// <param name="request">
    /// The query and paging parameters.
    /// </param>
    /// <returns>
    /// A page containing raw Examine search results.
    /// </returns>
    SearchResponseDto Search(SearchRequestDto request);
}

public sealed class ExamineSearchOptions
{
    public const string SectionName = "ExamineSearch";

    /// <summary>
    /// Gets or sets the Examine index name.
    /// </summary>
    public string IndexName { get; set; } =
        Constants.UmbracoIndexes.ExternalIndexName;

    /// <summary>
    /// Gets or sets the maximum allowed page size.
    /// </summary>
    public int MaximumPageSize { get; set; } = 100;
}

public sealed class ExamineSearchService(
    IExamineManager examineManager,
    IOptions<ExamineSearchOptions> options)
    : IExamineSearchService
{
    private static readonly HashSet<string> ReservedFieldNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            UmbracoExamineFieldNames.ItemIdFieldName,
            UmbracoExamineFieldNames.NodeKeyFieldName,
            UmbracoExamineFieldNames.IndexPathFieldName,
            UmbracoExamineFieldNames.IndexPathFieldName
        };

    private readonly ExamineSearchOptions _options = options.Value;

    /// <inheritdoc />
    public SearchResponseDto Search(SearchRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.QueryString);

        var page = ParsePositiveInteger(
            request.Page,
            defaultValue: 1);

        var pageSize = ParsePositiveInteger(
            request.PageSize,
            defaultValue: 10);

        pageSize = Math.Min(
            pageSize,
            Math.Max(1, _options.MaximumPageSize));

        if (!examineManager.TryGetIndex(
                _options.IndexName,
                out var index))
        {
            throw new InvalidOperationException(
                $"The Examine index '{_options.IndexName}' was not found.");
        }

        var skip = checked((page - 1) * pageSize);

        var searchResults = index.Searcher
            .CreateQuery(IndexTypes.Content)
            .NativeQuery(request.QueryString)
            .Execute(QueryOptions.SkipTake(skip, pageSize));

        var total = searchResults.TotalItemCount;

        var lastPage = total == 0
            ? 1
            : checked((int)Math.Ceiling(total / (double)pageSize));

        var normalizedRequest = request with
        {
            Page = page.ToString(),
            PageSize = pageSize.ToString()
        };

        IReadOnlyCollection<SearchResultDto> results = searchResults
            .Select(MapResult)
            .ToArray();

        return new SearchResponseDto
        {
            Request = normalizedRequest,
            Total = total.ToString(),
            FirstPage = "1",
            LastPage = lastPage.ToString(),
            CurrentPage = page.ToString(),
            Results = results
        };
    }

    private static SearchResultDto MapResult(ISearchResult searchResult)
    {
        var id = GetRequiredInt32(
            searchResult,
            UmbracoExamineFieldNames.ItemIdFieldName,
            searchResult.Id);
        
        var key = GetGuid(
            searchResult,
            UmbracoExamineFieldNames.NodeKeyFieldName);

        var path = GetPath(
            searchResult,
            UmbracoExamineFieldNames.IndexPathFieldName);

        var additionalProperties =
            searchResult.Values
                .Where(entry => !ReservedFieldNames.Contains(entry.Key))
                .ToDictionary(
                    entry => entry.Key,
                    entry => (object)entry.Value,
                    StringComparer.OrdinalIgnoreCase);
        return new SearchResultDto
        {
            ParentId = -1,
            Id = id,
            Key = key,
            Path = path,
            AdditionalProperties = additionalProperties
        };
    }

    private static object ConvertFieldValue(IReadOnlyList<string> values)
    {
        return values.Count switch
        {
            0 => string.Empty,
            1 => values[0],
            _ => values.ToArray()
        };
    }

    private static int GetRequiredInt32(
        ISearchResult searchResult,
        string fieldName,
        string? fallbackValue = null)
    {
        var value = GetFirstValue(
            searchResult,
            fieldName);

        value ??= fallbackValue;

        if (int.TryParse(value, out var parsedValue))
        {
            return parsedValue;
        }

        throw new InvalidOperationException(
            $"The Examine result '{searchResult.Id}' does not contain a " +
            $"valid integer value in field '{fieldName}'.");
    }

    private static int GetInt32(
        ISearchResult searchResult,
        string fieldName)
    {
        var value = GetFirstValue(
            searchResult,
            fieldName);

        return int.TryParse(value, out var parsedValue)
            ? parsedValue
            : 0;
    }

    private static Guid GetGuid(
        ISearchResult searchResult,
        string fieldName)
    {
        var value = GetFirstValue(
            searchResult,
            fieldName);

        return Guid.TryParse(value, out var parsedValue)
            ? parsedValue
            : Guid.Empty;
    }

    private static int[] GetPath(
        ISearchResult searchResult,
        string fieldName)
    {
        var value = GetFirstValue(
            searchResult,
            fieldName);

        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Select(segment =>
                int.TryParse(segment, out var id)
                    ? (int?)id
                    : null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToArray();
    }

    private static string? GetFirstValue(
        ISearchResult searchResult,
        string fieldName)
    {
        return searchResult.Values.TryGetValue(fieldName, out string? value)
            ? value
            : null;
    }

    private static int ParsePositiveInteger(
        string? value,
        int defaultValue)
    {
        return int.TryParse(value, out var parsedValue) &&
               parsedValue > 0
            ? parsedValue
            : defaultValue;
    }
}

public sealed record SearchRequestDto
{
    /// <summary>
    /// Gets the raw Examine/Lucene query.
    /// </summary>
    public required string QueryString { get; init; }

    /// <summary>
    /// Gets the requested one-based page number.
    /// </summary>
    public string Page { get; init; } = "1";

    /// <summary>
    /// Gets the requested page size.
    /// </summary>
    public string PageSize { get; init; } = "10";
}

public sealed record SearchResponseDto
{
    /// <summary>
    /// Gets the normalized search request.
    /// </summary>
    public required SearchRequestDto Request { get; init; }

    /// <summary>
    /// Gets the total number of matching results.
    /// </summary>
    public required string Total { get; init; }

    /// <summary>
    /// Gets the first available page.
    /// </summary>
    public required string FirstPage { get; init; }

    /// <summary>
    /// Gets the last available page.
    /// </summary>
    public required string LastPage { get; init; }

    /// <summary>
    /// Gets the current page.
    /// </summary>
    public required string CurrentPage { get; init; }

    /// <summary>
    /// Gets the results on the current page.
    /// </summary>
    public required IReadOnlyCollection<SearchResultDto> Results { get; init; }
}

public sealed record SearchResultDto
{
    /// <summary>
    /// Gets the parent content ID.
    /// </summary>
    public required int ParentId { get; init; }

    /// <summary>
    /// Gets the content ID.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the content key.
    /// </summary>
    public required Guid Key { get; init; }

    /// <summary>
    /// Gets the content path as an array of content IDs.
    /// </summary>
    public required int[] Path { get; init; }

    /// <summary>
    /// Gets all additional indexed fields.
    /// </summary>
    public required Dictionary<string, object> AdditionalProperties { get; init; }
}