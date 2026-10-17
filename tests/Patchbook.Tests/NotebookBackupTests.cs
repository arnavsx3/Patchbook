using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Patchbook.Entries;

namespace Patchbook.Tests;

public sealed class NotebookBackupTests
{
    [Fact]
    public async Task Backup_round_trip_preserves_content_identity_dates_and_pins()
    {
        var source = Store();
        var entry = await source.CreateAsync(Draft());
        entry = (await source.SetPinnedAsync(entry.Id, true))!;
        var bytes = await Service(source).ExportAsync();
        var destination = Store();
        Assert.Equal(new ImportResult(1, 0), await Service(destination).ImportAsync(bytes));
        var restored = Assert.Single(await destination.ListAsync());
        Assert.Equal(entry with { Tags = restored.Tags }, restored);
        Assert.Equal(entry.Tags.ToArray(), restored.Tags.ToArray());
        Assert.Equal(new ImportResult(0, 1), await Service(destination).ImportAsync(bytes));
        await destination.UpdateAsync(entry.Id, Draft("Keep my newer local fix"));
        await Service(destination).ImportAsync(bytes);
        Assert.Equal("Keep my newer local fix", (await destination.GetAsync(entry.Id))!.Title);
    }

    [Theory]
    [InlineData("bad-title")]
    [InlineData("unsafe-link")]
    [InlineData("duplicate-id")]
    [InlineData("empty-id")]
    [InlineData("bad-date")]
    [InlineData("null-entry")]
    [InlineData("null-tags")]
    [InlineData("null-fix")]
    [InlineData("missing-error")]
    [InlineData("future-version")]
    [InlineData("wrong-format")]
    public async Task Invalid_backups_never_partially_change_the_notebook(string invalid)
    {
        var source = Store();
        await source.CreateAsync(Draft());
        await source.CreateAsync(Draft("Second fix"));
        var document = JsonNode.Parse(await Service(source).ExportAsync())!;
        var entries = document["entries"]!.AsArray();
        var item = entries[1]!;
        switch (invalid)
        {
            case "bad-title": item["title"] = ""; break;
            case "unsafe-link": item["referenceUrl"] = "javascript:alert(1)"; break;
            case "duplicate-id": item["id"] = entries[0]!["id"]!.DeepClone(); break;
            case "empty-id": item["id"] = Guid.Empty.ToString(); break;
            case "bad-date": item["updatedAt"] = "2000-01-01T00:00:00Z"; break;
            case "null-entry": entries[1] = null; break;
            case "null-tags": item["tags"] = null; break;
            case "null-fix": item["fix"] = null; break;
            case "missing-error": item.AsObject().Remove("error"); break;
            case "future-version": document["version"] = 99; break;
            case "wrong-format": document["format"] = "another-app"; break;
        }
        var destination = Store();
        var existing = await destination.CreateAsync(Draft("Existing fix"));
        var exception = await Record.ExceptionAsync(() => Service(destination).ImportAsync(Encoding.UTF8.GetBytes(document.ToJsonString())));
        Assert.True(exception is ValidationException or JsonException);
        Assert.Equal(existing, Assert.Single(await destination.ListAsync()));
    }

    [Fact]
    public async Task Size_count_and_json_errors_are_reported_without_changes()
    {
        var store = Store();
        var service = Service(store);
        Assert.Equal(new ImportResult(0, 0), await service.ImportAsync(await service.ExportAsync()));
        await Assert.ThrowsAsync<ValidationException>(() => service.ImportAsync(new byte[NotebookBackupService.MaxBytes + 1]));
        await Assert.ThrowsAsync<JsonException>(() => service.ImportAsync(Encoding.UTF8.GetBytes("not json")));
        var source = Store();
        await source.CreateAsync(Draft());
        var document = JsonNode.Parse(await Service(source).ExportAsync())!;
        var item = document["entries"]![0]!.DeepClone();
        var entries = document["entries"]!.AsArray();
        entries.Clear();
        for (var i = 0; i <= NotebookBackupService.MaxEntries; i++) entries.Add(item.DeepClone());
        await Assert.ThrowsAsync<ValidationException>(() => service.ImportAsync(Encoding.UTF8.GetBytes(document.ToJsonString())));
        Assert.Empty(await store.ListAsync());
    }

    private static InMemoryEntryStore Store() => new(TimeProvider.System);
    private static NotebookBackupService Service(IEntryStore store) => new(store, TimeProvider.System);
    private static EntryDraft Draft(string title = "Backup fix") => new()
    {
        Title = title,
        Error = "Error",
        Fix = "Fix",
        Tags = "csharp, backups",
        Code = "  code\n",
        ReferenceUrl = "https://example.com"
    };
}
