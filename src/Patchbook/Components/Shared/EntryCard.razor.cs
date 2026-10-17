using Microsoft.AspNetCore.Components;
using Patchbook.Entries;

namespace Patchbook.Components.Shared;

public partial class EntryCard
{
    [Parameter, EditorRequired] public Entry Item { get; set; } = default!;
    [Parameter] public EventCallback<Entry> OnPinChanged { get; set; }
    [Parameter] public string SearchQuery { get; set; } = "";
    private SearchPreview Preview => EntrySearch.Preview(Item, SearchQuery);
    private string FirstLine => Item.Code.Split('\n').FirstOrDefault(line => !string.IsNullOrWhiteSpace(line)) ?? "";
}
