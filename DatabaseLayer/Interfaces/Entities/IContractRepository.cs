using DatabaseLayer.Models.KDO;

namespace DatabaseLayer.Interfaces.Entities
{
    public interface IContractRepository: IRepository<Contract>
    {
        int? SplitGenContract(int contractId, string user) { return 0; }
        bool CopyToArchiveDb(int contractId, string user, string sourceDB, string targetDB);
        bool RemoveArchivedContractData(int contractId, string user, string targetDB = "ContrArchiveTest");
    }
}
