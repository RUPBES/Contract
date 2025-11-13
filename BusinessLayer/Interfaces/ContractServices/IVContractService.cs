using BusinessLayer.Enums;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IVContractService: ILookupEntity<VContractDTO, VContract>
    {
        IEnumerable<VContractDTO> GetSubsByType(int? id, Enums.Contract? contractType, bool useArchiveData = false);
    }
}
