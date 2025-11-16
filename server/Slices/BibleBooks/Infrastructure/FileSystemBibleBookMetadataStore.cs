using System.Text.Json;
using Microsoft.Extensions.Options;
using Wtrfll.Server.Slices.BibleBooks.Application;
using Wtrfll.Server.Slices.BibleBooks.Domain;

namespace Wtrfll.Server.Slices.BibleBooks.Infrastructure;

public sealed class FileSystemBibleBookMetadataStore : IBibleBookMetadataStore
{
    private readonly Lazy<IReadOnlyList<BibleBookMetadata>> _metadata;

    public FileSystemBibleBookMetadataStore(
        IOptions<BibleBookMetadataOptions> options,
        IWebHostEnvironment hostingEnvironment,
        ILogger<FileSystemBibleBookMetadataStore> logger)
    {
        _metadata = new Lazy<IReadOnlyList<BibleBookMetadata>>(
            () => LoadMetadata(options.Value, hostingEnvironment.ContentRootPath, logger),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public IReadOnlyList<BibleBookMetadata> GetAll()
    {
        return _metadata.Value;
    }

    private static IReadOnlyList<BibleBookMetadata> LoadMetadata(
        BibleBookMetadataOptions options,
        string contentRoot,
        ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(options.DataFile))
        {
            throw new InvalidOperationException("BibleBooks:DataFile must be configured.");
        }

        var resolvedPath = Path.IsPathRooted(options.DataFile)
            ? options.DataFile
            : Path.Combine(contentRoot, options.DataFile);

        if (!File.Exists(resolvedPath))
        {
            throw new FileNotFoundException($"Bible book metadata file not found at {resolvedPath}", resolvedPath);
        }

        using var stream = File.OpenRead(resolvedPath);
        var payload = JsonSerializer.Deserialize<List<BibleBookMetadata>>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });

        if (payload is null || payload.Count == 0)
        {
            throw new InvalidOperationException($"Bible book metadata file at {resolvedPath} did not contain any entries.");
        }

        logger.LogInformation("Loaded {Count} bible book metadata entries from {File}", payload.Count, resolvedPath);
        return payload;
    }
}
