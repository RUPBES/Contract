using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractServices
{
    public interface IAdditionalTermService : IService<AdditionalTermDTO, AdditionalTerm>
    {
        void AddFile(int additionalTermId, int fileId);
    }
}
