using System.ComponentModel.DataAnnotations;

namespace Patchbook.Entries;

/// <summary>Editable input shared by the form and the store's validation boundary.</summary>
public sealed class EntryDraft : IValidatableObject
{
    [Required(ErrorMessage = "Give this fix a title.")]
    [StringLength(100)]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Describe the error or problem.")]
    [StringLength(8000)]
    public string Error { get; set; } = "";

    [StringLength(4000)]
    public string Cause { get; set; } = "";

    [Required(ErrorMessage = "Write down what fixed it.")]
    [StringLength(8000)]
    public string Fix { get; set; } = "";

    [StringLength(12000)]
    public string Code { get; set; } = "";

    [StringLength(32)]
    public string Language { get; set; } = "text";

    [StringLength(250)]
    public string Tags { get; set; } = "";

    [StringLength(2048)]
    public string ReferenceUrl { get; set; } = "";

    public string[] NormalizedTags() => Tags
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(tag => tag.ToLowerInvariant())
        .Distinct(StringComparer.Ordinal)
        .ToArray();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var tags = NormalizedTags();
        if (tags.Length > 8 || tags.Any(tag => tag.Length > 24))
        {
            yield return new ValidationResult(
                "Use up to 8 tags, each 24 characters or fewer.", [nameof(Tags)]);
        }

        if (!string.IsNullOrWhiteSpace(ReferenceUrl) &&
            (!Uri.TryCreate(ReferenceUrl.Trim(), UriKind.Absolute, out var uri) ||
             uri.Scheme is not ("https" or "http") || string.IsNullOrEmpty(uri.Host)))
        {
            yield return new ValidationResult(
                "Use a full http:// or https:// reference link.", [nameof(ReferenceUrl)]);
        }
    }

    public static EntryDraft From(Entry entry) => new()
    {
        Title = entry.Title,
        Error = entry.Error,
        Cause = entry.Cause,
        Fix = entry.Fix,
        Code = entry.Code,
        Language = entry.Language,
        Tags = string.Join(", ", entry.Tags),
        ReferenceUrl = entry.ReferenceUrl
    };
}
