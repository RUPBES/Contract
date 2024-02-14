using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IServiceGCService : IService<ServiceGCDTO, ServiceGc>
    {
        void AddAmendmentToService(int amendmentId, int serviceId);
    }
}
