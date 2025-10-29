using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using BusinessLayer.Models.Settings;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IPaymentService : IService<PaymentDTO, Payment>
    {
        IEnumerable<VPaymentCashDTO> GetPayableCash(FilterPayableModel filter, string[] organizationName);
        IndexViewModel GetPayableCash(int pageSize, int page, FilterPayableModel filter, string[] organizationName, bool? useArchiveData);
    }
}
