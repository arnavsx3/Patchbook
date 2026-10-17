namespace Patchbook.Entries;

public enum EntrySort { RecentlyUpdated, OldestFirst, Title }

public sealed record EntryQuery(string Search = "", string Tag = "", EntrySort Sort = EntrySort.RecentlyUpdated);

/// <summary>The UI depends on this contract so persistence can be added later.</summary>
public interface IEntryStore
{
    Task<IReadOnlyList<Entry>> ListAsync(EntryQuery? query = null);
    Task<Entry?> GetAsync(Guid id);
    Task<Entry> CreateAsync(EntryDraft draft);
    Task<Entry?> UpdateAsync(Guid id, EntryDraft draft);
    Task<bool> DeleteAsync(Guid id);
    Task<Entry?> SetPinnedAsync(Guid id, bool pinned);
}
