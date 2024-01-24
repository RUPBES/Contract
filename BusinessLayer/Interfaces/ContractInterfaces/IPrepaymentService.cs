using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IPrepaymentService : IService<PrepaymentDTO, Prepayment>
    {
        public IEnumerable<PrepaymentDTO> FindByContractId(int id);

        void AddAmendmentToPrepayment(int amendmentId, int prepaymentId);
        AmendmentDTO? GetAmendmentByPrepaymentId(int prepaymentId);
        IEnumerable<AmendmentDTO> GetFreeAmendment(int contractId);
        Prepayment GetLastPrepayment(int contractId);
    }
}