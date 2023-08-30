using DontCommitSecrets.Client.Models;

namespace DontCommitSecrets.Client.Services;

public interface IApiService
{
    Task<Section> Get();

    Task SaveSecret(Section section, string name, string value);

    Task RemoveSecret(Section section, string name);
}
