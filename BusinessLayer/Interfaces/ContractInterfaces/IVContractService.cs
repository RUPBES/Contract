using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IVContractService: ILookupEntity<VContractDTO, VContract>
    {
        public IEnumerable<VContractDTO> FindContract(string queryString);
        public IndexViewModel GetPageFilter(int pageSize, int pageNum, string request, string sortOrder, string org);
    }
}
