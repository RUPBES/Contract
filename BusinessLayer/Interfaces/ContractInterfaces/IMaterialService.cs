using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IMaterialService : IService<MaterialDTO, MaterialGc>
    {
        void AddAmendmentToMaterial(int amendmentId, int materialId);
    }
}
