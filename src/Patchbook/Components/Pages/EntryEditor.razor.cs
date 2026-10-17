using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Patchbook.Entries;

namespace Patchbook.Components.Pages;

public partial class EntryEditor
{
    [Inject] private IEntryStore Store { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Parameter] public Guid? Id { get; set; }
    private EntryDraft _draft = new();
    private EditContext? _context;
    private Guid? _loadedId;
    private bool _loaded, _missing, _saving, _saved;
    private string _error = "";
    private const string LabelClass = "mb-2 block text-sm font-semibold";
    private const string FieldClass = "w-full rounded-xl border border-line bg-white px-4 py-3 text-sm leading-6 placeholder:text-muted [&.invalid]:border-red-400";

    protected override async Task OnParametersSetAsync()
    {
        if (_loaded && _loadedId == Id) return;
        _loaded = true;
        _loadedId = Id;
        _saved = false;
        _missing = false;
        _error = "";
        _draft = new EntryDraft();
        if (Id.HasValue)
        {
            var entry = await Store.GetAsync(Id.Value);
            if (entry is null) { _missing = true; return; }
            _draft = EntryDraft.From(entry);
        }
        _context = new EditContext(_draft);
    }

    private async Task Save()
    {
        if (_saving) return;
        _saving = true;
        try
        {
            var entry = Id.HasValue
                ? await Store.UpdateAsync(Id.Value, _draft)
                : await Store.CreateAsync(_draft);
            if (entry is null)
            {
                _error = "This entry is no longer available. Copy your changes before leaving this page.";
                return;
            }
            _saved = true;
            Navigation.NavigateTo($"/entries/{entry.Id}");
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException exception)
        {
            _error = exception.Message;
        }
        finally { _saving = false; }
    }

    private async Task CheckNavigation(LocationChangingContext context)
    {
        if (!_saved && _context?.IsModified() == true && !await JS.InvokeAsync<bool>("patchbook.confirmLeave"))
        {
            context.PreventNavigation();
        }
    }
}
