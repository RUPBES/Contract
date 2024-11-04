using BusinessLayer.Models.PRO;

namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface ITextSearcher
    {
        string? SearchNumberWithEnd(string args);
        AbbreviationKindOfWorkDTO? SearchKindOfWork(string args);
    }
}
