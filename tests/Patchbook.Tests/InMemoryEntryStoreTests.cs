using System.ComponentModel.DataAnnotations;
using Patchbook.Entries;

namespace Patchbook.Tests;

public sealed class InMemoryEntryStoreTests
{
    private readonly TestClock _clock = new();

    private static EntryDraft Draft(string title = "Fix the connection") => new()
    {
        Title = title,
        Error = "Connection refused",
        Cause = "Wrong service port",
        Fix = "Use port 5080",
        Code = "  dotnet run\n",
        Language = "bash",
        Tags = " .NET, networking, .net, ",
        ReferenceUrl = "https://learn.microsoft.com/dotnet/"
    };

    [Fact]
    public async Task Create_normalizes_input_and_keeps_code_whitespace()
    {
        var store = new InMemoryEntryStore(_clock);
        var draft = Draft("  A useful fix  ");
        var entry = await store.CreateAsync(draft);
        Assert.Equal("A useful fix", entry.Title);
        Assert.Equal(new[] { ".net", "networking" }, entry.Tags.ToArray());
        Assert.Equal("  dotnet run\n", entry.Code);
        Assert.Equal(_clock.GetUtcNow(), entry.CreatedAt);
        Assert.Equal(entry.CreatedAt, entry.UpdatedAt);
        Assert.Equal(entry, await store.GetAsync(entry.Id));

        draft.Title = "Changed outside the store";
        Assert.Equal("A useful fix", (await store.GetAsync(entry.Id))!.Title);
    }

    [Fact]
    public async Task Update_preserves_identity_and_creation_time_without_mutating_previous_snapshot()
    {
        var store = new InMemoryEntryStore(_clock);
        var original = await store.CreateAsync(Draft());
        _clock.Advance(TimeSpan.FromHours(1));
        var updated = await store.UpdateAsync(original.Id, Draft("An improved fix"));
        Assert.NotNull(updated);
        Assert.Equal(original.Id, updated.Id);
        Assert.Equal(original.CreatedAt, updated.CreatedAt);
        Assert.Equal(_clock.GetUtcNow(), updated.UpdatedAt);
        Assert.Equal("Fix the connection", original.Title);
        Assert.Equal("An improved fix", updated.Title);
    }

    [Theory]
    [InlineData("connection")]
    [InlineData("REFUSED")]
    [InlineData("wrong service")]
    [InlineData("port 5080")]
    [InlineData("dotnet run")]
    [InlineData("networking")]
    [InlineData("bash")]
    [InlineData("learn.microsoft.com")]
    public async Task Search_matches_every_saved_field_case_insensitively(string search)
    {
        var store = new InMemoryEntryStore(_clock);
        var entry = await store.CreateAsync(Draft());
        Assert.Equal(entry, Assert.Single(await store.ListAsync(new EntryQuery(search))));
    }

    [Fact]
    public async Task Tag_filter_combines_with_search_and_matches_whole_tags()
    {
        var store = new InMemoryEntryStore(_clock);
        await store.CreateAsync(Draft());
        Assert.Single(await store.ListAsync(new EntryQuery("connection", " .NET ")));
        Assert.Empty(await store.ListAsync(new EntryQuery("connection", "net")));
        Assert.Empty(await store.ListAsync(new EntryQuery("unrelated", ".net")));
    }

    [Fact]
    public async Task Sorting_uses_update_time_creation_time_or_title()
    {
        var store = new InMemoryEntryStore(_clock);
        var zebra = await store.CreateAsync(Draft("Zebra"));
        _clock.Advance(TimeSpan.FromMinutes(1));
        var alpha = await store.CreateAsync(Draft("Alpha"));
        _clock.Advance(TimeSpan.FromMinutes(1));
        await store.UpdateAsync(zebra.Id, Draft("Zebra"));

        Assert.Equal(zebra.Id, (await store.ListAsync())[0].Id);
        Assert.Equal(zebra.Id, (await store.ListAsync(new EntryQuery(Sort: EntrySort.OldestFirst)))[0].Id);
        Assert.Equal(alpha.Id, (await store.ListAsync(new EntryQuery(Sort: EntrySort.Title)))[0].Id);
    }

    [Theory]
    [InlineData("title")]
    [InlineData("error")]
    [InlineData("fix")]
    [InlineData("long-title")]
    [InlineData("too-many-tags")]
    [InlineData("long-tag")]
    [InlineData("long-code")]
    public async Task Invalid_entries_are_rejected_without_changing_existing_data(string invalidField)
    {
        var store = new InMemoryEntryStore(_clock);
        var original = await store.CreateAsync(Draft());
        var invalid = Draft();
        switch (invalidField)
        {
            case "title": invalid.Title = " "; break;
            case "error": invalid.Error = ""; break;
            case "fix": invalid.Fix = ""; break;
            case "long-title": invalid.Title = new string('x', 101); break;
            case "too-many-tags": invalid.Tags = "a,b,c,d,e,f,g,h,i"; break;
            case "long-tag": invalid.Tags = new string('x', 25); break;
            case "long-code": invalid.Code = new string('x', 12001); break;
        }

        await Assert.ThrowsAsync<ValidationException>(() => store.CreateAsync(invalid));
        await Assert.ThrowsAsync<ValidationException>(() => store.UpdateAsync(original.Id, invalid));
        Assert.Equal(original, Assert.Single(await store.ListAsync()));
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("file:///etc/passwd")]
    [InlineData("example.com")]
    [InlineData("/relative")]
    public async Task Reference_links_only_allow_absolute_http_or_https_urls(string url)
    {
        var store = new InMemoryEntryStore(_clock);
        var draft = Draft();
        draft.ReferenceUrl = url;
        await Assert.ThrowsAsync<ValidationException>(() => store.CreateAsync(draft));
        Assert.Empty(await store.ListAsync());
    }

    [Fact]
    public async Task Missing_entries_are_handled_and_deletion_removes_the_entry()
    {
        var store = new InMemoryEntryStore(_clock);
        var id = Guid.NewGuid();
        Assert.Null(await store.GetAsync(id));
        Assert.Null(await store.UpdateAsync(id, Draft()));
        Assert.False(await store.DeleteAsync(id));
        var entry = await store.CreateAsync(Draft());
        Assert.True(await store.DeleteAsync(entry.Id));
        Assert.Null(await store.GetAsync(entry.Id));
        Assert.Empty(await store.ListAsync());
    }

    [Fact]
    public async Task Concurrent_creates_keep_all_entries_and_a_new_store_starts_empty()
    {
        var store = new InMemoryEntryStore(_clock);
        await Parallel.ForEachAsync(Enumerable.Range(0, 100), async (i, _) =>
        {
            await store.CreateAsync(Draft($"Fix {i}"));
        });
        var entries = await store.ListAsync();
        Assert.Equal(100, entries.Count);
        Assert.Equal(100, entries.Select(entry => entry.Id).Distinct().Count());
        Assert.Empty(await new InMemoryEntryStore(_clock).ListAsync());
    }

    private sealed class TestClock : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan duration) => _now += duration;
    }
}
