namespace Wtrfll.Server.Slices.Passages.Application;

public interface IPassageProvider
{
    bool CanHandle(string translationCode);

    Task<PassageResultDto?> GetPassageAsync(string translationCode, string reference, CancellationToken cancellationToken);
}


