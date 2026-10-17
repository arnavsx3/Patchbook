using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Patchbook.Entries;

namespace Patchbook.Components.Pages;

public partial class Home
{
    [Inject] private IEntryStore Store { get; set; } = default!;
    private IReadOnlyList<Entry> _allEntries = [];
    private IReadOnlyList<Entry> _entries = [];
    private string _search = "";
    private string _tag = "";
    private string _language = "";
    private EntrySort _sort;
    private bool _loadingExamples;
    private bool HasFilters => _search.Length > 0 || _tag.Length > 0 || _language.Length > 0;
    private IEnumerable<string> Languages => _allEntries.Select(entry => entry.Language)
        .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(CodeLanguages.LabelFor, StringComparer.OrdinalIgnoreCase);
    private IEnumerable<string> Tags => _allEntries.SelectMany(entry => entry.Tags).Distinct().Order();

    protected override Task OnInitializedAsync() => Load();

    private async Task Load()
    {
        _allEntries = await Store.ListAsync();
        _entries = await Store.ListAsync(new EntryQuery(_search, _tag, _sort, _language));
    }

    private async Task SetTag(string tag) { _tag = tag; await Load(); }
    private Task ClearTag() => SetTag("");
    private Task OnPinChanged(Entry _) => Load();
    private Task OnImported(ImportResult _) => Load();
    private async Task ClearFilters() { _search = ""; _tag = ""; _language = ""; await Load(); }

    private static string TagClass(bool selected) =>
        "max-w-full break-words rounded-lg border px-3 py-1.5 font-mono text-[11px] transition " +
        (selected ? "border-violet-200 bg-violet-100 text-brand" : "border-line bg-white text-muted hover:border-violet-200 hover:text-brand");

    private async Task LoadExamples()
    {
        if (_loadingExamples) return;
        _loadingExamples = true;
        if ((await Store.ListAsync()).Count == 0)
        {
            foreach (var draft in ExampleEntries.All)
            {
                await Store.CreateAsync(draft);
            }
        }
        await Load();
        _loadingExamples = false;
    }
}
