#if DEBUG
using DontCommitSecrets.Client.Models;

namespace DontCommitSecrets.Client.Services;

public class MockApiService : IApiService
{
    public Task<Section> Get()
    {
        var rootSection = new Section()
        {
            Title = "Root",
        };
        rootSection.SubSections = new[]
        {
            new Section()
            {
                Title = "SubSection",
                Path = "SubSection",
                Parent = rootSection,
                Entries = new Dictionary<string, object>()
                {
                    { "key", "value" },
                    { "anotherKey", "5" }
                }
            }
        };

        return Task.FromResult(rootSection);
    }

    public Task RemoveSecret(Section section, string name)
    {
        return Task.CompletedTask;
    }

    public Task SaveSecret(Section section, string name, string value)
    {
        return Task.CompletedTask;
    }
}
#endif
