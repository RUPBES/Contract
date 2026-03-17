using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces.Dapper
{
    public interface IReadonlyContractDapperRepo : IReadonlyRepoDapper<Contract>
    {
        IEnumerable<VContract> GetSubsById(int id, string where, string? databaseName = null);
        VContract GetById(string whereStr, string? databaseName = null);
    }
}