using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces.Dapper
{
    public interface IReadonlyPaymentDapperRepo : IReadonlyRepoDapper<VPaymentCash>
    {
        IEnumerable<VPaymentCash> GetEntitySkipTake(int skip, int take, string queryWhere, string[] orgList, string? databaseName = null);
        (IEnumerable<VPaymentCash>, int) Filter(int skip, int take, string org, string? query, string? orderBy);
    }
}
