using BusinessLayer.Models.PRO;

namespace BusinessLayer.Interfaces.Shared
{
    public interface ITextSearcher
    {
        string? SearchNumberWithEnd(string args);
        string? SearchNumberWithStart(string args);
        AbbreviationKindOfWorkDTO? SearchKindOfWork(string args);
    }
}
