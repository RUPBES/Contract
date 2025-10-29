using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IContractOrganizationService:IService<ContractOrganizationDTO, ContractOrganization>
    {
        ContractOrganizationDTO GetById(int id, int? secondId, bool? useArchiveData);
    }
}
