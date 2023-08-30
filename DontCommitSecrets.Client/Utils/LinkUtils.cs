using DontCommitSecrets.Client.Models;

namespace DontCommitSecrets.Client.Utils;

public static class LinkUtils
{
    public static string? GetSectionLink(Section section)
    {
        return '#' + section.Path?.Replace(SectionUtils.SectionSeparator, "-");
    }
}
