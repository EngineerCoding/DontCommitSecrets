using Microsoft.Extensions.Configuration;

namespace DontCommitSecrets;

public static class IConfigurationExtensions
{
    public static void AddDontCommitSecrets(this IConfigurationBuilder builder, string url, TimeSpan? ttl = null)
    {
        builder.Add(new DontCommitSecretsSource(url, ttl));
    }

    public static void AddDontCommitSecrets(this IConfigurationBuilder builder, Uri url, TimeSpan? ttl = null)
    {
        builder.Add(new DontCommitSecretsSource(url, ttl));
    }
}
