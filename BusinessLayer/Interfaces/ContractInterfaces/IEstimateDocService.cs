using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IEstimateDocService : IService<EstimateDocDTO, EstimateDoc>
    {
        void AddFile(int estimateDocId, int fileId);
    }
}
