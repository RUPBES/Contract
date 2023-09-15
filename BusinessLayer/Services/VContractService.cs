using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;

namespace BusinessLayer.Services
{
    internal class VContractService : IVContractService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        public VContractService(IContractUoW database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public IEnumerable<VContractDTO> Find(Func<VContract, bool> predicate)
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_database.vContracts.Find(predicate));
        }

        public IEnumerable<VContractDTO> FindContract(string queryString)
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_database.vContracts.FindContract(queryString));
        }

        public IEnumerable<VContractDTO> FindLikeNameObj(string queryString)
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_database.vContracts.FindLikeNameObj(queryString));
        }

        public IEnumerable<VContractDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_database.vContracts.GetAll());
        }

        public VContractDTO GetById(int id)
        {
            var contract = _database.vContracts.GetById(id);

            if (contract is not null)
            {
                return _mapper.Map<VContractDTO>(contract);
            }
            else
            {
                return null;
            }
        }

        public IndexViewModel GetPage(int pageSize, int pageNum)
        {
            int count = _database.vContracts.Count();
            int skipEntities = (pageNum - 1) * pageSize;
            var items = _database.vContracts.GetEntitySkipTake(skipEntities, pageSize);
            var t = _mapper.Map<IEnumerable<VContractDTO>>(items);

            PageViewModel pageViewModel = new PageViewModel(count, pageNum, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = t
            };

            return viewModel;
        }

        public IndexViewModel GetPageFilter(int pageSize, int pageNum, string request, string sortOrder, bool isEngin)
        {
            int count = _database.vContracts.Count();
            int skipEntities = (pageNum - 1) * pageSize;
            IEnumerable<VContract> items;
            if (!String.IsNullOrEmpty(request))
            {      items = _database.vContracts.FindContract(request); }
            else { items = _database.vContracts.GetAll(); }


            switch (sortOrder)
            {
                case "date":
                    items = items.OrderBy(s => s.Date).ThenBy(s => s.Number);
                    break;
                case "dateDesc":
                    items = items.OrderByDescending(s => s.Date).ThenBy(s => s.Number);
                    break;
                case "nameObject":
                    items = items.OrderBy(s => s.NameObject).ThenBy(s => s.Id);
                    break;
                case "nameObjectDesc":
                    items = items.OrderByDescending(s => s.NameObject).ThenBy(s => s.Id);
                    break;
                case "client":
                    items = items.OrderBy(s => s.Client).ThenBy(s => s.Id);
                    break;
                case "clientDesc":
                    items = items.OrderByDescending(s => s.Client).ThenBy(s => s.Id);
                    break;
                case "genContractor":
                    items = items.OrderBy(s => s.GenContractor).ThenBy(s => s.Id);
                    break;
                case "genContractorDesc":
                    items = items.OrderByDescending(s => s.GenContractor).ThenBy(s => s.Id);
                    break;
                case "dateEnter":
                    items = items.OrderBy(s => s.EnteringTerm).ThenBy(s => s.Id); 
                    break;
                case "dateEnterDesc":
                    items = items.OrderByDescending(s => s.EnteringTerm).ThenBy(s => s.Id);
                    break;
                default:
                    items = items.OrderBy(s => s.Id);
                    break;
            }
            items.Skip(skipEntities).Take(pageSize);
            var t = _mapper.Map<IEnumerable<VContractDTO>>(items);

            PageViewModel pageViewModel = new PageViewModel(count, pageNum, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = t
            };

            return viewModel;
        }
    }
}
