namespace Wtrfll.Server.Slices.Passages.Application;

public sealed class PassageReadService
{
    private readonly IEnumerable<IPassageProvider> _providers;

    public PassageReadService(IEnumerable<IPassageProvider> providers)
    {
        _providers = providers;
    }

    public async Task<PassageResultDto?> GetPassageAsync(string translationCode, string reference, CancellationToken cancellationToken)
    {
        var provider = _providers.FirstOrDefault(p => p.CanHandle(translationCode));
        if (provider is null)
        {
            return null;
        }

        return await provider.GetPassageAsync(translationCode, reference, cancellationToken);
    }
}



