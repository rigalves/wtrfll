using Wtrfll.Server.Slices.Passages.Application;

namespace Wtrfll.Server.Tests.Common;

public sealed class FakePassageProvider : IPassageProvider
{
    private readonly string _translationCode;
    private readonly PassageResultDto _result;

    public FakePassageProvider(string translationCode = "FAKE", string reference = "John 3:16")
    {
        _translationCode = translationCode;
        _result = new PassageResultDto
        {
            Reference = reference,
            Translation = translationCode,
            Verses = new List<VerseDto>
            {
                new VerseDto { Book = "John", Chapter = 3, Verse = 16, Text = "For God so loved the world" }
            },
            Attribution = new AttributionDto { Required = false, Text = "Test" },
            CachePolicy = "no-store",
        };
    }

    public bool CanHandle(string translationCode) => string.Equals(translationCode, _translationCode, StringComparison.OrdinalIgnoreCase);

    public Task<PassageResultDto?> GetPassageAsync(string translationCode, string reference, CancellationToken cancellationToken)
    {
        if (!CanHandle(translationCode))
        {
            return Task.FromResult<PassageResultDto?>(null);
        }

        return Task.FromResult<PassageResultDto?>(_result with { Reference = reference });
    }
}
