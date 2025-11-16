using Wtrfll.Server.Slices.Bibles.Domain;

namespace Wtrfll.Server.Slices.Bibles.Application;

public interface IBibleTranslationCatalog
{
    IReadOnlyList<BibleTranslation> GetAll();
}



