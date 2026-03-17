using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces.Dapper
{
    public interface IReadonlyVContractDapperRepo : IReadOnlyRepository<VContract>
    {
        IEnumerable<VContract> GetSubsById(int id, string where);
        VContract GetById(string whereStr);
    }
}
