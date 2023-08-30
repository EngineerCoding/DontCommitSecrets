using System.ComponentModel.DataAnnotations;

namespace DontCommitSecrets.WebApp.Models;

public record Secret
{
    [Required]
    public object Value { get; set; } = null!;

    [Required]
    public string Key { get; set; } = null!;
}
