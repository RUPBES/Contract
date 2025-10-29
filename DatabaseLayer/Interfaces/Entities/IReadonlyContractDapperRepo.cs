using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces.Entities
{
    public interface IReadonlyContractDapperRepo : IReadonlyRepoDapper<Contract>
    {
        IEnumerable<VContract> GetSubsById(int id, string where);
        VContract GetById(string whereStr);
    }
}