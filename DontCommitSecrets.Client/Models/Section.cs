using System.ComponentModel.DataAnnotations;

namespace DontCommitSecrets.Client.Models;

public record Section
{
    [Required]
    public string Title { get; set; } = null!;

    public string? Path { get; set; }

    public Section? Parent { get; set; }

    public Dictionary<string, object> Entries { get; set; } = new Dictionary<string, object>();

    public IEnumerable<Section> SubSections { get; set; } = Enumerable.Empty<Section>();
}
