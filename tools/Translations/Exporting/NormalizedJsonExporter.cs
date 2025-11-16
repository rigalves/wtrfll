using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Wtrfll.Translations.Shared;

namespace Wtrfll.Translations.Exporting;

public sealed class NormalizedJsonExporter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public async Task ExportAsync(NormalizedBible bible, string outputPath)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(outputPath);
        await JsonSerializer.SerializeAsync(stream, bible, SerializerOptions);
    }
}
