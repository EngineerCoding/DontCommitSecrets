using System.Text.Json;
using System.Text.Json.Nodes;

namespace DontCommitSecrets.Client.Utils;

public static class JsonUtils
{
    public static IDictionary<string, object> ToNativeValueDictionary(IDictionary<string, object> jsonData)
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

    public static object? ToNativeValue(object? data)
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
