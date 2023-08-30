using DontCommitSecrets.WebApp.Utils;

namespace DontCommitSecrets.WebApp.Services;

public class StorageService : IStorageService
{
    private readonly IEnumerable<ISecretSyncService> _syncServices;
    private readonly SemaphoreHelper _mutexHelper = new SemaphoreHelper();
    private IDictionary<string, object>? _secrets;

    public StorageService(IEnumerable<ISecretSyncService> syncServices)
    {
        _syncServices = syncServices.ToArray();
    }

    public Task<IDictionary<string, object>> GetSecrets(CancellationToken cancellationToken)
    {
        return _mutexHelper.Run(async () =>
        {
            var getSecretsTasks = _syncServices.Select(service => service.GetSecrets(cancellationToken));
            await Task.WhenAll(getSecretsTasks);

            _secrets = new Dictionary<string, object>();
            var dataDictionaries = getSecretsTasks.Select(task => task.Result).ToArray();
            foreach (var kvp in dataDictionaries.SelectMany(dict => dict.AsEnumerable()))
            {
                _secrets[kvp.Key] = kvp.Value;
            }

            if (dataDictionaries.Length > 1)
            {
                var syncTasks = _syncServices.Select(service => service.StoreSecrets(_secrets, CancellationToken.None));
                await Task.WhenAll(syncTasks);
            }

            return _secrets;
        });
    }

    public Task StoreSecrets(IDictionary<string, object> data, CancellationToken cancellationToken)
    {
        var actualData = JsonUtils.ToNativeValueDictionary(data);

        return _mutexHelper.Run(async () =>
        {
            var tasks = _syncServices.Select(service => service.StoreSecrets(actualData, CancellationToken.None));
            await Task.WhenAll(tasks);
            _secrets = actualData;
        });
    }

    public Task StoreSecret(string key, object value, CancellationToken cancellationToken)
    {
        var actualValue = JsonUtils.ToNativeValue(value);
        if (actualValue == null)
        {
            return Task.CompletedTask;
        }

        return _mutexHelper.Run(async () =>
        {
            var tasks = _syncServices.Select(service => service.StoreSecret(key, actualValue, CancellationToken.None));
            await Task.WhenAll(tasks);

            if (_secrets != null)
            {
                _secrets[key] = value;
            }
        });
    }

    public Task RemoveSecret(string key, CancellationToken cancellationToken)
    {
        return _mutexHelper.Run(async () =>
        {
            var tasks = _syncServices.Select(service => service.RemoveSecret(key, CancellationToken.None));
            await Task.WhenAll(tasks);

            if (_secrets != null)
            {
                _secrets.Remove(key);
            }
        });
    }
}
