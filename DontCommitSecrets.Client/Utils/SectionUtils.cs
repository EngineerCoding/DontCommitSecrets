using DontCommitSecrets.Client.Models;

namespace DontCommitSecrets.Client.Utils;

public static class SectionUtils
{
    public static readonly string SectionSeparator = ConfigurationPath.KeyDelimiter;

    public static Section ToRootSection(this IDictionary<string, object> data)
    {
        var rootSection = new Section()
        {
            Title = "Root"
        };
        var sectionMapping = new Dictionary<string, Section>();

        data = JsonUtils.ToNativeValueDictionary(data);
        foreach (var key in data.Keys)
        {
            var sectionComponents = key.Split(SectionSeparator);

            Section section;
            if (sectionComponents.Length > 1)
            {
                var sectionKey = string.Join(SectionSeparator, sectionComponents.Take(sectionComponents.Length - 1));
                if (!sectionMapping.TryGetValue(sectionKey, out var subSection))
                {
                    sectionMapping[sectionKey] = subSection = new Section()
                    {
                        Title = sectionComponents[sectionComponents.Length - 2],
                        Path = sectionKey,
                    };
                }

                section = subSection;
            }
            else
            {
                section = rootSection;
            }

            section.Entries.Add(sectionComponents[sectionComponents.Length - 1], data[key]);
        }

        return LinkSections(rootSection, sectionMapping);
    }

    private static Section LinkSections(Section rootSection, IDictionary<string, Section> subSections)
    {
        var sectionTree = new Dictionary<string, Section>();
        foreach (var kvp in subSections)
        {
            if (sectionTree.ContainsKey(kvp.Key))
            {
                var intermediateSection = sectionTree[kvp.Key];
                // Copy the links
                kvp.Value.SubSections = intermediateSection.SubSections;
                // Replace the referred parents
                foreach (var toReplaceParent in intermediateSection.SubSections)
                    toReplaceParent.Parent = kvp.Value;
                // Replace the item in the section tree
                sectionTree[kvp.Key] = kvp.Value;
                continue;
            }

            // Build up the section tree, since this particular sections was not created yet

            var sections = kvp.Key.Split(SectionSeparator);
            // Incrementally check the sections
            Section previous = rootSection;
            for (int i = 0; i < sections.Length - 1; i++) // excludes to last section
            {
                var sectionKey = string.Join(SectionSeparator, sections.Take(i + 1));
                if (!sectionTree.TryGetValue(sectionKey, out var subSection))
                {
                    subSection = new Section()
                    {
                        Title = sections[i],
                        Path = sectionKey,
                    };

                    sectionTree[sectionKey] = subSection;
                    subSection.Parent = previous;
                    previous.SubSections = previous.SubSections.Concat(new[] { subSection }).ToArray();
                }
                previous = subSection;
            }

            sectionTree[kvp.Key] = kvp.Value;
            sectionTree[kvp.Key].Parent = previous;
            previous.SubSections = previous.SubSections.Concat(new[] { kvp.Value }).ToArray();
        }

        return rootSection;
    }

    public static string ConstructPath(params string[] items)
    {
        return string.Join(SectionSeparator, items.Where(item => !string.IsNullOrEmpty(item)));
    }
}
