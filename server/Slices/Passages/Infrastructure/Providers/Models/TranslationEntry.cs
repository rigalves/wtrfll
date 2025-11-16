using System.Text.Json;
using Wtrfll.Server.Slices.Passages.Infrastructure.Providers.Models.Documents;

namespace Wtrfll.Server.Slices.Passages.Infrastructure.Providers.Models;

internal sealed class TranslationEntry
{
    private readonly NormalizedTranslationSource _source;
    private readonly string _filePath;
    private readonly Lazy<Task<NormalizedTranslationData>> _dataLoader;

    public TranslationEntry(NormalizedTranslationSource source, string filePath)
    {
        _source = source;
        _filePath = filePath;
        _dataLoader = new Lazy<Task<NormalizedTranslationData>>(LoadAsync);
    }

    public string CachePolicy => string.IsNullOrWhiteSpace(_source.CachePolicy) ? "no-store" : _source.CachePolicy;
    public bool AttributionRequired => _source.AttributionRequired;
    public string? AttributionText => _source.AttributionText;
    public string? AttributionUrl => _source.AttributionUrl;

    public Task<NormalizedTranslationData> GetDataAsync() => _dataLoader.Value;

    private async Task<NormalizedTranslationData> LoadAsync()
    {
        await using var stream = File.OpenRead(_filePath);
        var document = await JsonSerializer.DeserializeAsync<NormalizedBibleDocument>(stream, SerializerOptions)
            ?? throw new InvalidOperationException($"Invalid normalized translation file at {_filePath}");

        return NormalizedTranslationData.Create(document);
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}
