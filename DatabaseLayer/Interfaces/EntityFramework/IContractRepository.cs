using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces.EntityFramework
{
    public interface IContractRepository: IRepository<Contract>
    {
        int? SplitGenContract(int contractId, string user) { return 0; }
        bool CopyToArchiveDb(int contractId, string user, string sourceDB, string targetDB);
        bool RemoveContractData(int contractId, string user, string targetDB = "ContrArchiveTest");
    }
}
