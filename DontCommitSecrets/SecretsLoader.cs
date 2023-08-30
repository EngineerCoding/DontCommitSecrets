using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DontCommitSecrets;

internal static class SecretsLoader
{
    private static readonly HttpClient _httpClient = new HttpClient();

    private static readonly ConcurrentDictionary<Uri, (IDictionary<string, object> Data, DateTime Loaded)> _loadedData
        = new ConcurrentDictionary<Uri, (IDictionary<string, object>, DateTime)>();

    public static IDictionary<string, object> LoadData(Uri uri, TimeSpan cacheTTL)
    {
        var currentDateTime = DateTime.UtcNow;

        if (_loadedData.TryGetValue(uri, out var cachedData) && cachedData.Loaded - currentDateTime < cacheTTL)
        {
            return cachedData.Data;
        }

        var data = _httpClient.GetFromJsonAsync<IDictionary<string, object>>(uri).GetAwaiter().GetResult();
        if (data == null)
        {
            throw new Exception("Could not load secrets data");
        }

        data = ToNativeValueDictionary(data);
        _loadedData[uri] = (data, DateTime.UtcNow);
        return data;
    }

    private static IDictionary<string, object> ToNativeValueDictionary(IDictionary<string, object> jsonData)
    {
        var actualData = new Dictionary<string, object>();
        foreach (var kvp in jsonData)
        {
            var value = ToNativeValue(kvp.Value);
            if (value == null)
            {
                continue;
            }

            actualData[kvp.Key] = value;
        }

        return actualData;
    }

    private static object? ToNativeValue(object? data)
    {
        if (data is JsonElement jsonElement &&
                jsonElement.ValueKind != JsonValueKind.Object && jsonElement.ValueKind != JsonValueKind.Array)
        {
            data = JsonValue.Create(jsonElement);
        }
        if (data == null)
            return null;

        if (data is JsonValue jsonValue)
        {
            if (jsonValue.TryGetValue(out DateTime dateTime)) return dateTime;
            if (jsonValue.TryGetValue(out Guid guid)) return guid;
            if (jsonValue.TryGetValue(out bool boolValue)) return boolValue;
            if (jsonValue.TryGetValue(out int intValue)) return intValue;
            if (jsonValue.TryGetValue(out double doubleValue)) return doubleValue;
            if (jsonValue.TryGetValue(out string? stringValue)) return stringValue;
        }

        throw new Exception("Unexpected data: " + data);
    }
}
