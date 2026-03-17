using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IVContractEnginService : ILookupEntity<VContractDTO, VContractEngin>
    {
        IndexViewModel Filter(int pageSize, int pageNum, string type, string? sortDirection, string org, string? searchText, string? whereCondition, bool? useArchiveData);
    }
}