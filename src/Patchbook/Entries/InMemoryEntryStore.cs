using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace Patchbook.Entries;

/// <summary>
/// One local notebook shared by browser connections. All entries disappear on restart.
/// The lock protects writes and snapshots; callers only receive immutable records.
/// </summary>
public sealed class InMemoryEntryStore(TimeProvider clock) : IEntryStore
{
    private readonly Dictionary<Guid, Entry> _entries = [];
    private readonly Lock _gate = new();

    public Task<IReadOnlyList<Entry>> ListAsync(EntryQuery? query = null)
    {
        query ??= new EntryQuery();
        Entry[] snapshot;
        lock (_gate)
        {
            snapshot = _entries.Values.ToArray();
        }

        var search = query.Search.Trim();
        var tag = query.Tag.Trim();
        var entries = snapshot.Where(entry =>
            (tag.Length == 0 || entry.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)) &&
            (search.Length == 0 || SearchableFields(entry)
                .Any(field => field.Contains(search, StringComparison.OrdinalIgnoreCase))));

        var ordered = query.Sort switch
        {
            EntrySort.Title => entries.OrderBy(entry => entry.Title, StringComparer.OrdinalIgnoreCase),
            EntrySort.OldestFirst => entries.OrderBy(entry => entry.CreatedAt),
            _ => entries.OrderByDescending(entry => entry.UpdatedAt)
        };

        // LINQ ordering is stable, so pins retain the requested order within each group.
        return Task.FromResult<IReadOnlyList<Entry>>(ordered.ThenBy(entry => entry.Id)
            .OrderByDescending(entry => entry.IsPinned).ToArray());
    }

    public Task<Entry?> GetAsync(Guid id)
    {
        lock (_gate)
        {
            return Task.FromResult(_entries.GetValueOrDefault(id));
        }
    }

    public Task<Entry> CreateAsync(EntryDraft draft)
    {
        Validate(draft);
        var now = clock.GetUtcNow();
        var entry = Snapshot(Guid.NewGuid(), draft, now, now);
        lock (_gate)
        {
            _entries.Add(entry.Id, entry);
        }
        return Task.FromResult(entry);
    }

    public Task<Entry?> UpdateAsync(Guid id, EntryDraft draft)
    {
        Validate(draft);
        lock (_gate)
        {
            if (!_entries.TryGetValue(id, out var existing))
            {
                return Task.FromResult<Entry?>(null);
            }

            var entry = Snapshot(id, draft, existing.CreatedAt, clock.GetUtcNow()) with { IsPinned = existing.IsPinned };
            _entries[id] = entry;
            return Task.FromResult<Entry?>(entry);
        }
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        lock (_gate)
        {
            return Task.FromResult(_entries.Remove(id));
        }
    }

    public Task<Entry?> SetPinnedAsync(Guid id, bool pinned)
    {
        lock (_gate)
        {
            if (!_entries.TryGetValue(id, out var entry)) return Task.FromResult<Entry?>(null);
            var updated = entry with { IsPinned = pinned };
            _entries[id] = updated;
            return Task.FromResult<Entry?>(updated);
        }
    }

    private static void Validate(EntryDraft draft) =>
        Validator.ValidateObject(draft, new ValidationContext(draft), validateAllProperties: true);

    public Task<ImportResult> ImportAsync(IReadOnlyList<Entry> entries)
    {
        // Validate and normalize the entire batch before changing the notebook.
        var batch = new List<Entry>();
        var ids = new HashSet<Guid>();
        foreach (var entry in entries)
        {
            if (entry is null || entry.Id == Guid.Empty || !ids.Add(entry.Id) ||
                entry.CreatedAt == default || entry.UpdatedAt < entry.CreatedAt ||
                entry.Tags.IsDefault || entry.Tags.Any(string.IsNullOrWhiteSpace) ||
                new[] { entry.Title, entry.Error, entry.Cause, entry.Fix, entry.Code, entry.Language, entry.ReferenceUrl }.Any(value => value is null))
            {
                throw new ValidationException("The backup contains invalid entry data or repeated IDs.");
            }
            var draft = EntryDraft.From(entry);
            Validate(draft);
            batch.Add(Snapshot(entry.Id, draft, entry.CreatedAt, entry.UpdatedAt) with { IsPinned = entry.IsPinned });
        }
        lock (_gate)
        {
            var added = batch.Count(entry => _entries.TryAdd(entry.Id, entry));
            return Task.FromResult(new ImportResult(added, batch.Count - added));
        }
    }

    private static Entry Snapshot(Guid id, EntryDraft draft, DateTimeOffset created, DateTimeOffset updated) =>
        new(id, draft.Title.Trim(), draft.Error.Trim(), draft.Cause.Trim(), draft.Fix.Trim(),
            draft.Code, draft.Language.Trim(), draft.NormalizedTags().ToImmutableArray(),
            draft.ReferenceUrl.Trim(), created, updated);

    private static IEnumerable<string> SearchableFields(Entry entry) =>
        [entry.Title, entry.Error, entry.Cause, entry.Fix, entry.Code, entry.Language, entry.ReferenceUrl, .. entry.Tags];
}
