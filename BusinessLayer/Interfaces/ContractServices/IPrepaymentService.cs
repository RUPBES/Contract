using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models.Extra;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IPrepaymentService : IService<PrepaymentDTO, Prepayment>
    {
        public IEnumerable<PrepaymentDTO> FindByContractId(int id, bool? useArchiveData = null);

        void AddAmendmentToPrepayment(int amendmentId, int prepaymentId);
        //AmendmentDTO? GetAmendmentByPrepaymentId(int prepaymentId);
        IEnumerable<AmendmentDTO> GetFreeAmendment(int contractId);
        Prepayment GetLastPrepayment(int contractId, bool? useArchiveData = null);
        Prepayment GetPrepaymentByAmendment(int amendmentId);

        Task<PrepaymentScheduleDTO>? GetPeriodAdvancesAsync(int contractId, DateTime startPeriod, DateTime endPeriod, bool? useArchiveData = null);

        //работа с полученными авансами
        void FillReceived(PrepaymentReceivedDTO item);
        IEnumerable<PrepaymentReceivedDTO> FindRecieved(Func<PrepaymentReceived, bool> predicate, bool? useArchiveData = null);
    }
}