using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces.Entities
{
    public interface IReadonlyPaymentDapperRepo : IReadonlyRepoDapper<VPaymentCash>
    {
        IEnumerable<VPaymentCash> GetEntitySkipTake(int skip, int take, string queryWhere, string[] orgList);
    }
}
