using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces.Dapper
{
    public interface IReadonlyContractDapperRepo : IReadonlyRepoDapper<Contract>
    {
        IEnumerable<VContract> GetSubsById(int id, string where, string? databaseName = null);
        (IEnumerable<VContract> genClientContracts, IEnumerable<VContract> subContracts) GetByOrganizationId(int orgId, string? databaseName);
        VContract GetById(string whereStr, string? databaseName = null);
    }
}