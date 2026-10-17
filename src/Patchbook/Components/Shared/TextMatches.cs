namespace Patchbook.Components.Shared;

public sealed record TextMatch(string Text, bool IsMatch);

/// <summary>Literal, case-insensitive segments; Razor remains responsible for HTML encoding.</summary>
public static class TextMatches
{
    public static IReadOnlyList<TextMatch> Split(string text, string search)
    {
        var query = search.Trim();
        if (query.Length == 0) return [new(text, false)];
        var segments = new List<TextMatch>();
        var cursor = 0;
        while (cursor < text.Length)
        {
            var index = text.IndexOf(query, cursor, StringComparison.OrdinalIgnoreCase);
            if (index < 0) break;
            if (index > cursor) segments.Add(new(text[cursor..index], false));
            segments.Add(new(text.Substring(index, query.Length), true));
            cursor = index + query.Length;
        }
        if (cursor < text.Length) segments.Add(new(text[cursor..], false));
        return segments;
    }
}
