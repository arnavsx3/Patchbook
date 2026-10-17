using System.Collections.Immutable;

namespace Patchbook.Entries;

public sealed record CodeLanguage(string Id, string Label);

public static class CodeLanguages
{
    public static ImmutableArray<CodeLanguage> All { get; } =
    [
        new("text", "Plain text"), new("csharp", "C#"), new("bash", "Shell"),
        new("javascript", "JavaScript"), new("typescript", "TypeScript"),
        new("python", "Python"), new("elixir", "Elixir"), new("sql", "SQL"),
        new("json", "JSON"), new("html", "HTML"), new("css", "CSS")
    ];

    public static string LabelFor(string id) =>
        All.FirstOrDefault(language => language.Id.Equals(id, StringComparison.OrdinalIgnoreCase))?.Label ?? id;
}
