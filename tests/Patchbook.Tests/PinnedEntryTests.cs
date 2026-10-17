using Patchbook.Entries;

namespace Patchbook.Tests;

public sealed class PinnedEntryTests
{
    [Theory]
    [InlineData(EntrySort.Title)]
    [InlineData(EntrySort.RecentlyUpdated)]
    [InlineData(EntrySort.OldestFirst)]
    public async Task Pinned_entries_come_first_and_keep_the_requested_order(EntrySort sort)
    {
        var store = new InMemoryEntryStore(TimeProvider.System);
        var z = await store.CreateAsync(Draft("Zebra"));
        var a = await store.CreateAsync(Draft("Alpha"));
        var m = await store.CreateAsync(Draft("Middle"));
        await store.SetPinnedAsync(a.Id, true);
        await store.SetPinnedAsync(z.Id, true);
        var entries = await store.ListAsync(new EntryQuery(Sort: sort));
        Assert.All(entries.Take(2), entry => Assert.True(entry.IsPinned));
        Assert.Equal(m.Id, entries[2].Id);
        if (sort == EntrySort.Title) Assert.Equal(a.Id, entries[0].Id);
        await store.SetPinnedAsync(z.Id, false);
        Assert.False((await store.GetAsync(z.Id))!.IsPinned);
    }

    [Fact]
    public async Task Pinning_does_not_change_edit_time_and_editing_keeps_the_pin()
    {
        var store = new InMemoryEntryStore(TimeProvider.System);
        var entry = await store.CreateAsync(Draft("Original"));
        var pinned = await store.SetPinnedAsync(entry.Id, true);
        Assert.Equal(entry.UpdatedAt, pinned!.UpdatedAt);
        Assert.True((await store.UpdateAsync(entry.Id, Draft("Edited")))!.IsPinned);
        Assert.Null(await store.SetPinnedAsync(Guid.NewGuid(), true));
    }

    private static EntryDraft Draft(string title) => new() { Title = title, Error = "Error", Fix = "Fix" };
}
