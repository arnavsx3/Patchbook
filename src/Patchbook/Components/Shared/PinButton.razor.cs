using Microsoft.AspNetCore.Components;
using Patchbook.Entries;

namespace Patchbook.Components.Shared;

public partial class PinButton
{
    [Inject] private IEntryStore Store { get; set; } = default!;
    [Parameter, EditorRequired] public Entry Item { get; set; } = default!;
    [Parameter] public EventCallback<Entry> OnChanged { get; set; }
    private bool _busy;

    private async Task Toggle()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            var updated = await Store.SetPinnedAsync(Item.Id, !Item.IsPinned);
            if (updated is not null) await OnChanged.InvokeAsync(updated);
        }
        finally { _busy = false; }
    }
}
