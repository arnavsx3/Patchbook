namespace Patchbook.Entries;

/// <summary>Optional examples, only added when the user chooses to load them.</summary>
public static class ExampleEntries
{
    public static IEnumerable<EntryDraft> All =>
    [
        new()
        {
            Title = "Read a missing setting safely",
            Error = "A missing configuration key caused a null reference exception.",
            Cause = "The configuration indexer returns null when a key does not exist.",
            Fix = "Use a fallback for optional values. For required values, fail early with a clear error instead.",
            Code = "var title = configuration[\"Notebook:Title\"] ?? \"My notebook\";",
            Language = "csharp", Tags = "csharp, dotnet, configuration",
            ReferenceUrl = "https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-coalescing-operator"
        },
        new()
        {
            Title = "Fix a typo in the last Git commit",
            Error = "I committed the right changes with the wrong message.",
            Cause = "The commit message contained a typo.",
            Fix = "Before pushing, amend the latest local commit. This rewrites the commit, so use it for an unshared commit.",
            Code = "git commit --amend -m \"docs: clarify setup instructions\"",
            Language = "bash", Tags = "git, workflow",
            ReferenceUrl = "https://git-scm.com/docs/git-commit"
        },
        new()
        {
            Title = "Wait for every asynchronous operation",
            Error = "The page rendered before all requests had finished.",
            Cause = "The asynchronous operations were started but their completion was not awaited.",
            Fix = "Collect the tasks and await Task.WhenAll before using their results.",
            Code = "var tasks = urls.Select(url => client.GetStringAsync(url));\nvar pages = await Task.WhenAll(tasks);",
            Language = "csharp", Tags = "csharp, async",
            ReferenceUrl = "https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall"
        }
    ];
}
