using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IPrepaymentService : IService<PrepaymentDTO, Prepayment>
    {
        public IEnumerable<PrepaymentDTO> FindListByIdContract(int id);
    }
}