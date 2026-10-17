using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Patchbook.Entries;

namespace Patchbook.Components.Shared;

public partial class CodeBlock
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Parameter, EditorRequired] public string Code { get; set; } = "";
    [Parameter] public string Language { get; set; } = "text";
    private bool _copied;
    private string _message = "";

    private async Task Copy()
    {
        _copied = await JS.InvokeAsync<bool>("patchbook.copy", Code);
        _message = _copied ? "Code copied to clipboard." : "Clipboard unavailable. Select the code and copy it manually.";
    }
}
