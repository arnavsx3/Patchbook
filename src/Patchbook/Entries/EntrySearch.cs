namespace Patchbook.Entries;

public sealed record SearchPreview(string Label, string Text);

/// <summary>Shared search fields keep filtering and visible match previews consistent.</summary>
public static class EntrySearch
{
    public static bool Matches(Entry entry, string search) => string.IsNullOrWhiteSpace(search) ||
        Fields(entry).Any(field => field.Text.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));

    public static SearchPreview Preview(Entry entry, string search)
    {
        var query = search.Trim();
        if (query.Length == 0 || entry.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            return new("Problem", entry.Error);

        var field = Fields(entry).FirstOrDefault(field => field.Text.Contains(query, StringComparison.OrdinalIgnoreCase));
        if (field.Text is null) return new("Problem", entry.Error);
        var index = field.Text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        var start = Math.Max(0, index - 45);
        var end = Math.Min(field.Text.Length, index + query.Length + 80);
        return new(field.Label, (start > 0 ? "…" : "") + field.Text[start..end] + (end < field.Text.Length ? "…" : ""));
    }

    private static IEnumerable<(string Label, string Text)> Fields(Entry entry) =>
    [
        ("Title", entry.Title), ("Problem", entry.Error), ("Cause", entry.Cause),
        ("Fix", entry.Fix), ("Code", entry.Code), ("Language", entry.Language),
        ("Language", CodeLanguages.LabelFor(entry.Language)), ("Reference", entry.ReferenceUrl),
        .. entry.Tags.Select(tag => ("Tag", tag))
    ];
}
