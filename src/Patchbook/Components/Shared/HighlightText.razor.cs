using Microsoft.AspNetCore.Components;

namespace Patchbook.Components.Shared;

public partial class HighlightText
{
    [Parameter] public string Text { get; set; } = "";
    [Parameter] public string Query { get; set; } = "";
    private IReadOnlyList<TextMatch> _segments = [];
    protected override void OnParametersSet() => _segments = TextMatches.Split(Text, Query);
}
