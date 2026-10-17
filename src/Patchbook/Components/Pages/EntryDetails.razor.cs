using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Patchbook.Entries;

namespace Patchbook.Components.Pages;

public partial class EntryDetails
{
    [Inject] private IEntryStore Store { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Parameter] public Guid Id { get; set; }
    private Entry? _entry;
    private ElementReference _deleteDialog;
    protected override async Task OnParametersSetAsync() => _entry = await Store.GetAsync(Id);
    private async Task ShowDelete() => await JS.InvokeVoidAsync("patchbook.showDialog", _deleteDialog);
    private async Task CloseDelete() => await JS.InvokeVoidAsync("patchbook.closeDialog", _deleteDialog);
    private async Task Delete()
    {
        await Store.DeleteAsync(Id);
        Navigation.NavigateTo("/");
    }
}
