using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Patchbook.Entries;

namespace Patchbook.Components.Shared;

public partial class BackupTools
{
    [Inject] private NotebookBackupService Backups { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Parameter] public EventCallback<ImportResult> OnImported { get; set; }
    private bool _busy;
    private string _message = "";
    private int _inputKey;

    private async Task Export()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            var bytes = await Backups.ExportAsync();
            await JS.InvokeVoidAsync("patchbook.downloadJson", $"patchbook-{DateTime.UtcNow:yyyy-MM-dd}.json", bytes);
            _message = "Backup downloaded. Keep it somewhere safe, then import it whenever you need your fixes.";
        }
        catch (ValidationException exception) { _message = exception.Message; }
        catch (JSException) { _message = "The download could not start. Please try again."; }
        finally { _busy = false; }
    }

    private async Task Import(InputFileChangeEventArgs args)
    {
        if (_busy) return;
        _busy = true;
        _message = "Reading backup…";
        try
        {
            if (args.File.Size > NotebookBackupService.MaxBytes)
                throw new ValidationException("Backups must be 2 MB or smaller.");
            await using var stream = args.File.OpenReadStream(NotebookBackupService.MaxBytes);
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);
            var result = await Backups.ImportAsync(buffer.ToArray());
            await OnImported.InvokeAsync(result);
            _message = $"Imported {result.Added} entries. Skipped {result.Skipped} already saved. Existing entries were kept.";
        }
        catch (ValidationException exception) { _message = exception.Message; }
        catch (JsonException) { _message = "Could not read this backup. Choose a valid Patchbook JSON export."; }
        catch (Exception exception) when (exception is IOException or JSException or OperationCanceledException)
        {
            _message = "The file could not be read. Please choose it again.";
        }
        finally { _busy = false; _inputKey++; }
    }
}
