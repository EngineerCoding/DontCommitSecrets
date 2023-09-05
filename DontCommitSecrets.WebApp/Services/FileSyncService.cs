using DontCommitSecrets.Client.Utils;
using DontCommitSecrets.WebApp.Utils;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DontCommitSecrets.WebApp.Services;

public class FileSyncService : ISecretSyncService
{
    private static readonly string SecretsFile = Path.Join(AppContext.BaseDirectory, "data", "dont_commit_this_file");
    private SemaphoreHelper _mutexHelper = new SemaphoreHelper();
    private ConcurrentDictionary<string, object>? _data;

    public async Task<IDictionary<string, object>> GetSecrets(CancellationToken cancellationToken)
    {
        return await _mutexHelper.Run(() => GetData(cancellationToken));
    }

    public Task StoreSecrets(IDictionary<string, object> data, CancellationToken cancellationToken)
    {
        return _mutexHelper.Run(async () =>
        {
            _data = new ConcurrentDictionary<string, object>(data.AsEnumerable());
            await PersistToDisk(cancellationToken);
        });
    }

    public Task StoreSecret(string key, object value, CancellationToken cancellationToken)
    {
        return _mutexHelper.Run(async () =>
        {
            _data ??= await GetData(cancellationToken);
            _data.AddOrUpdate(key, value, (_, _) => value);
            await PersistToDisk(cancellationToken);
        });
    }

    public Task RemoveSecret(string key, CancellationToken cancellationToken)
    {
        return _mutexHelper.Run(async () =>
        {
            _data?.Remove(key, out _);
            await PersistToDisk(cancellationToken);
        });
    }

    private async Task<ConcurrentDictionary<string, object>> GetData(CancellationToken cancellationToken)
    {
        if (_data != null)
        {
            return _data;
        }

        var concurrentDictionary = new ConcurrentDictionary<string, object>();
        if (File.Exists(SecretsFile))
        {
            using var stream = File.OpenRead(SecretsFile);
            var jsonObject = await JsonSerializer.DeserializeAsync<JsonObject>(stream, cancellationToken: cancellationToken);
            if (jsonObject == null)
                return concurrentDictionary;

            ReadToDictionary(concurrentDictionary, new Stack<string>(), jsonObject);
        }
        return concurrentDictionary;
    }

    private static void ReadToDictionary(ConcurrentDictionary<string, object> target, Stack<string> path, JsonObject jsonObject)
    {
        var basicPath = SectionUtils.ConstructPath(path.Reverse().ToArray());
        foreach (var kvp in jsonObject)
        {
            if (kvp.Value is JsonObject subObject)
            {
                path.Push(kvp.Key);
                ReadToDictionary(target, path, subObject);
                path.Pop();
            }
            else
            {
                var value = Client.Utils.JsonUtils.ToNativeValue(kvp.Value);
                if (value == null)
                    continue;

                var key = SectionUtils.ConstructPath(basicPath, kvp.Key);
                target[key] = value;
            }
        }
    }

    private async Task PersistToDisk(CancellationToken cancellationToken)
    {
        if (_data == null)
        {
            if (File.Exists(SecretsFile))
                File.Delete(SecretsFile);
            return;
        }

        var directory = Path.GetDirectoryName(SecretsFile);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        using var stream = File.OpenWrite(SecretsFile);
        await JsonSerializer.SerializeAsync(stream, GetFormattedData(), cancellationToken: cancellationToken);
    }

    private JsonObject GetFormattedData()
    {
        var data = new JsonObject();
        if (_data == null)
            return data;

        foreach (var kvp in _data)
        {
            var splitKey = kvp.Key.Split(SectionUtils.SectionSeparator);

            var targetJsonObject = data;
            if (splitKey.Length > 1)
            {
                foreach (var subJsonKey in splitKey.Take(splitKey.Length - 1))
                {
                    if (!targetJsonObject.ContainsKey(subJsonKey))
                    {
                        targetJsonObject[subJsonKey] = new JsonObject();
                    }
                    targetJsonObject = (JsonObject)targetJsonObject[subJsonKey]!;
                }
            }
            targetJsonObject[splitKey.Last()] = JsonValue.Create(kvp.Value);
        }

        return data;
    }
}
