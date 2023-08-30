using DontCommitSecrets.Configuration;
using Microsoft.Extensions.Configuration;

namespace DontCommitSecrets;

public class DontCommitSecretsSource : IConfigurationSource
{
    public Uri Endpoint { get; }

    public TimeSpan CacheTTL { get; }

    public DontCommitSecretsSource(string host, TimeSpan? cacheTTL) : this(new Uri(host), cacheTTL)
    {
    }

    public DontCommitSecretsSource(Uri host, TimeSpan? cacheTTL)
    {
        var uriBuilder = new UriBuilder(host);

        var currentPath = uriBuilder.Path.TrimEnd('/');
        if (!currentPath.EndsWith("/api/secrets"))
        {
            currentPath += "/api/secrets";
        }
        uriBuilder.Path = currentPath;

        Endpoint = uriBuilder.Uri;
        CacheTTL = cacheTTL ?? TimeSpan.FromMinutes(5);
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new DontCommitSecretsProvider(this);
    }
}
