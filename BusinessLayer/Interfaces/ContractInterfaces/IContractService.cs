﻿using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IContractService : IService<ContractDTO, Contract>
    {
        List<ContractDTO>? ExistContractAndReturnListSameContracts(string numberContract, DateTime? dateContract);
        bool ExistContractByNumber(string numberContract);

        void AddFile(int contractId, int fileId);
        public IEnumerable<ContractDTO> GetPageFilter(int pageSize, int pageNum, string request, string filter, out int count);
        public IEnumerable<ContractDTO> GetPage(int pageSize, int pageNum, string filter, out int count);
    }
}