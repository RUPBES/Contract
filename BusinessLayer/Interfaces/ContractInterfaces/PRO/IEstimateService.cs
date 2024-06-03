﻿using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using BusinessLayer.Models.PRO;
using DatabaseLayer.Models.PRO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IEstimateService : IService<EstimateDTO, Estimate>
    {
        public IndexViewModel GetPage(int pageSize, int pageNum, string org);
        public IndexViewModel GetPageFilter(int pageSize, int pageNum, string request, string sortOrder, string org);
        public IndexViewModel GetPageFilterByContract(int pageSize, int pageNum, string request, string sortOrder, int ContractId, List<string> KeySearchString, List<string> ValueSearchString);
        public List<DatabaseLayer.Models.KDO.File> GetFiles(int EstimateId);
    }
}