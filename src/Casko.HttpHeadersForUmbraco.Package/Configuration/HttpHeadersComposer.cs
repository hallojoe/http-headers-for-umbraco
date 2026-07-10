using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Casko.HttpHeadersForUmbraco.Package.Configuration;

public sealed class HttpHeadersComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddHttpResponseHeaders(builder.Config);
    }
}
