using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using System.Diagnostics;

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

        public IndexViewModel GetPage(int pageSize, int pageNum, string org)
        {            
            int skipEntities = (pageNum - 1) * pageSize;
           
            var items = _database.vContracts.GetEntitySkipTake(skipEntities, pageSize, org);
            
            int count = items.Count();
            foreach (var item in items)
            {
                var amend = _database.Amendments.Find(x => x.ContractId == item.Id).OrderBy(x => x.Date).
                    Select(x => new Amendment {DateBeginWork = x.DateBeginWork, DateEndWork = x.DateEndWork, DateEntryObject = x.DateEntryObject }).LastOrDefault();
                if (amend is not null)
                {
                    item.DateBeginWork = amend.DateBeginWork;
                    item.DateEndWork = amend.DateEndWork;
                    item.EnteringTerm = amend.DateEntryObject;
                }
            }
            var t = _mapper.Map<IEnumerable<VContractDTO>>(items);

            PageViewModel pageViewModel = new PageViewModel(count, pageNum, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = t
            };
           
            return viewModel;
        }

        public IndexViewModel GetPageFilter(int pageSize, int pageNum, string request, string typeRequest, string sortOrder, string org)
        {
            var list = org.Split(',');
            int skipEntities = (pageNum - 1) * pageSize;
            IEnumerable<VContract> items;
            if (!String.IsNullOrEmpty(request))
            {
                switch (typeRequest)
                {
                    case "number":
                        items = _database.vContracts.Find(x => list.Contains(x.Owner) && x.Number != null && x.Number.Contains(request));
                        break;
                    case "nameObject":
                        items = _database.vContracts.Find(x => list.Contains(x.Owner) && x.NameObject != null && x.NameObject.Contains(request));
                        break;
                    case "client":
                        items = _database.vContracts.Find(x => list.Contains(x.Owner) && x.Client != null && x.Client.Contains(request));
                        break;
                    case "general":
                        items = _database.vContracts.Find(x => list.Contains(x.Owner) && x.GenContractor != null && x.GenContractor.Contains(request));
                        break;                   
                    default:
                        items = _database.vContracts.Find(x => list.Contains(x.Owner));
                        break;
                }                
            }
            else 
            { 
                items = _database.vContracts.Find(x => list.Contains(x.Owner)); 
            }
            int count = items.Count();

            switch (sortOrder)
            {
                case "number":
                    items = items.OrderBy(s => s.Date).ThenBy(s => s.Number);
                    break;
                case "numberDesc":
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

            items = items.Skip(skipEntities).Take(pageSize);
            foreach (var item in items)
            {
                var amend = _database.Amendments.Find(x => x.ContractId == item.Id).OrderBy(x => x.Date).
                    Select(x => new Amendment { DateBeginWork = x.DateBeginWork, DateEndWork = x.DateEndWork, DateEntryObject = x.DateEntryObject }).LastOrDefault();
                if (amend is not null)
                {
                    item.DateBeginWork = amend.DateBeginWork;
                    item.DateEndWork = amend.DateEndWork;
                    item.EnteringTerm = amend.DateEntryObject;
                }
            }
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
