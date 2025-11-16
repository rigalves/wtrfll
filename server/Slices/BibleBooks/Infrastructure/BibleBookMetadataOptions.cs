using System.ComponentModel.DataAnnotations;

namespace Wtrfll.Server.Slices.BibleBooks.Infrastructure;

public sealed class BibleBookMetadataOptions
{
    public const string SectionName = "BibleBooks";

    [Required]
    public string DataFile { get; init; } = string.Empty;
}
