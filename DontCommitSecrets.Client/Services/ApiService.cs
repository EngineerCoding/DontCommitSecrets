using DontCommitSecrets.Client.Models;
using DontCommitSecrets.Client.Utils;
using System.Net.Http.Json;

namespace DontCommitSecrets.Client.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Section> Get()
    {
        var data = await _httpClient.GetFromJsonAsync<IDictionary<string, object>>("/api/secrets");
        if (data == null)
        {
            throw new Exception("Unable to load data");
        }

        return data.ToRootSection();
    }

    public async Task SaveSecret(Section section, string name, string value)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/secret");
        httpRequest.Content = JsonContent.Create(new Secret(
            SectionUtils.ConstructPath(section.Path!, name), value));

        var httpResponse = await _httpClient.SendAsync(httpRequest);
        httpResponse.EnsureSuccessStatusCode();
    }

    public async Task RemoveSecret(Section section, string name)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/secret?secretKey={section.Path!}");

        var httpResponse = await _httpClient.SendAsync(httpRequest);
        httpResponse.EnsureSuccessStatusCode();
    }

    private record SecretKey(string Key);

    private record Secret(string Key, string Value);
}
