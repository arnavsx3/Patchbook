using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Patchbook.Entries;

public sealed record NotebookBackup(string Format, int Version, DateTimeOffset ExportedAt, IReadOnlyList<Entry> Entries);

/// <summary>Versioned, bounded JSON backups. Imports add missing IDs and never overwrite entries.</summary>
public sealed class NotebookBackupService(IEntryStore store, TimeProvider clock)
{
    public const int MaxBytes = 2 * 1024 * 1024;
    public const int MaxEntries = 1000;
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 16
    };

    public async Task<byte[]> ExportAsync()
    {
        var entries = await store.ListAsync();
        if (entries.Count > MaxEntries) throw new ValidationException("Backups support up to 1,000 entries.");
        var bytes = JsonSerializer.SerializeToUtf8Bytes(new NotebookBackup("patchbook", 1, clock.GetUtcNow(), entries), Options);
        CheckSize(bytes);
        return bytes;
    }

    public Task<ImportResult> ImportAsync(byte[] bytes)
    {
        CheckSize(bytes);
        var backup = JsonSerializer.Deserialize<NotebookBackup>(bytes, Options);
        if (backup is null || backup.Format != "patchbook" || backup.Version != 1 ||
            backup.ExportedAt == default || backup.Entries is null || backup.Entries.Count > MaxEntries)
        {
            throw new ValidationException("Choose a supported Patchbook backup with up to 1,000 entries.");
        }
        return store.ImportAsync(backup.Entries);
    }

    private static void CheckSize(byte[] bytes)
    {
        if (bytes.Length > MaxBytes) throw new ValidationException("Backups must be 2 MB or smaller.");
    }
}
