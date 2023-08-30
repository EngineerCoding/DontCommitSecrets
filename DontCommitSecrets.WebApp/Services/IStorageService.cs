namespace DontCommitSecrets.WebApp.Services;

public interface IStorageService
{
    Task<IDictionary<string, object>> GetSecrets(CancellationToken cancellationToken);

    Task StoreSecrets(IDictionary<string, object> data, CancellationToken cancellationToken);

    Task StoreSecret(string key, object value, CancellationToken cancellationToken);

    Task RemoveSecret(string key, CancellationToken cancellationToken);
}
