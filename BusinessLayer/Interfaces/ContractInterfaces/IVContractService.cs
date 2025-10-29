using BusinessLayer.Enums;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IVContractService: ILookupEntity<VContractDTO, VContract>
    {
        IEnumerable<VContractDTO> GetSubsByType(int? id, ContractType? contractType, bool useArchiveData = false);
    }
}
