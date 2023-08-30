using Microsoft.Extensions.Configuration;

namespace DontCommitSecrets.Configuration;

public class DontCommitSecretsProvider : ConfigurationProvider
{
    private DontCommitSecretsSource _configurationSource;

    public DontCommitSecretsProvider(DontCommitSecretsSource configurationSource)
    {
        _configurationSource = configurationSource;
    }

    public override void Load()
    {
        var secrets = SecretsLoader.LoadData(_configurationSource.Endpoint, _configurationSource.CacheTTL);
        foreach (var secret in secrets)
        {
            Data[secret.Key] = secret.Value.ToString();
        }
    }
}
