using Patchbook.Components.Shared;
using Patchbook.Entries;

namespace Patchbook.Tests;

public sealed class EntrySearchTests
{
    [Fact]
    public async Task Language_filter_combines_with_tags_search_and_pin_order()
    {
        var store = new InMemoryEntryStore(TimeProvider.System);
        var csharp = await store.CreateAsync(new EntryDraft { Title = "First", Error = "token issue", Fix = "Fix", Tags = "backend", Language = "csharp" });
        var shell = await store.CreateAsync(new EntryDraft { Title = "Second", Error = "token issue", Fix = "Fix", Tags = "backend", Language = "bash" });
        await store.SetPinnedAsync(shell.Id, true);
        Assert.Equal(csharp.Id, Assert.Single(await store.ListAsync(new EntryQuery("token", "backend", Language: " CSHARP "))).Id);
        Assert.Empty(await store.ListAsync(new EntryQuery("token", "frontend", Language: "csharp")));
        Assert.Empty(await store.ListAsync(new EntryQuery(Language: "sharp")));
        Assert.Equal(csharp.Id, Assert.Single(await store.ListAsync(new EntryQuery("C#"))).Id);
    }

    [Fact]
    public async Task Preview_shows_context_for_a_match_deep_in_a_solution()
    {
        var store = new InMemoryEntryStore(TimeProvider.System);
        var entry = await store.CreateAsync(new EntryDraft { Title = "A fix", Error = "Problem", Fix = new string('a', 200) + "needle" + new string('z', 200) });
        var preview = EntrySearch.Preview(entry, "NEEDLE");
        Assert.Equal("Fix", preview.Label);
        Assert.Contains("needle", preview.Text);
        Assert.StartsWith("…", preview.Text);
        Assert.EndsWith("…", preview.Text);
        Assert.True(preview.Text.Length < 140);
        Assert.Equal("Problem", EntrySearch.Preview(entry, "A fix").Text);
    }

    [Theory]
    [InlineData("A+b and a+B", "a+b", 2)]
    [InlineData("<img src=x>", "<img", 1)]
    [InlineData("abc", " ", 0)]
    [InlineData("abc", "xyz", 0)]
    [InlineData("", "x", 0)]
    [InlineData("dotnet DOTNET", " dotnet ", 2)]
    public void Match_segments_preserve_text_and_treat_search_as_literal(string text, string query, int count)
    {
        var segments = TextMatches.Split(text, query);
        Assert.Equal(text, string.Concat(segments.Select(segment => segment.Text)));
        Assert.Equal(count, segments.Count(segment => segment.IsMatch));
    }
}
