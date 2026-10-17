using System.Collections.Immutable;

namespace Patchbook.Entries;

/// <summary>An immutable snapshot of a saved fix.</summary>
public sealed record Entry(
    Guid Id,
    string Title,
    string Error,
    string Cause,
    string Fix,
    string Code,
    string Language,
    ImmutableArray<string> Tags,
    string ReferenceUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    bool IsPinned = false);
