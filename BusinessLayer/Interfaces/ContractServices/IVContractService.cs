using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IVContractService: ILookupEntity<VContractDTO, VContract>
    {
        IEnumerable<VContractDTO> GetSubsByType(int? id, Enums.ContractType? contractType, bool useArchiveData = false);
        IndexViewModel Filter(int pageSize, int pageNum, string type, string? sortDirection, string org, string? searchText, string? whereCondition = null, bool? useArchiveData = null);
    }
}
