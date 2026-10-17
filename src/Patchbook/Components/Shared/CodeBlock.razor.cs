using System.Net;
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
    private string? _highlighted;
    private bool _highlightPending;
    private MarkupString RenderedCode => new(_highlighted ?? WebUtility.HtmlEncode(Code));
    private string? _lastCode, _lastLanguage;
    private int LineCount => Code.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').Length;
    private string LineNumbers => string.Join('\n', Enumerable.Range(1, LineCount));

    protected override void OnParametersSet()
    {
        if (_lastCode == Code && _lastLanguage == Language) return;
        _lastCode = Code;
        _lastLanguage = Language;
        _highlighted = null;
        _highlightPending = true;
        _copied = false;
        _message = "";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_highlightPending) return;
        _highlightPending = false;
        var code = Code;
        var language = Language;
        try
        {
            var html = await JS.InvokeAsync<string>("patchbook.highlight", code, language);
            if (code != Code || language != Language) return;
            _highlighted = html;
            StateHasChanged();
        }
        catch (JSException) { /* Plain text remains available if highlighting cannot load. */ }
    }

    private async Task Copy()
    {
        _copied = await JS.InvokeAsync<bool>("patchbook.copy", Code);
        _message = _copied ? "Code copied to clipboard." : "Clipboard unavailable. Select the code and copy it manually.";
    }
}
