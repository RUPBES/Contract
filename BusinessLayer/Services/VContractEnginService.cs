using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Services
{
    internal class VContractEnginService : IVContractEnginService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        public VContractEnginService(IContractUoW database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public IEnumerable<VContractDTO> Find(Func<VContractEngin, bool> predicate)
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_database.vContractEngins.Find(predicate));
        }

        public IEnumerable<VContractDTO> FindContract(string queryString)
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_database.vContractEngins.FindContract(queryString));
        }

        public IEnumerable<VContractDTO> FindLikeNameObj(string queryString)
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_database.vContractEngins.FindLikeNameObj(queryString));
        }

        public IEnumerable<VContractDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_database.vContractEngins.GetAll());
        }

        public VContractDTO GetById(int id)
        {
            var contract = _database.vContractEngins.GetById(id);

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
            var items = _database.vContractEngins.GetEntitySkipTake(skipEntities, pageSize, org);
            int count = items.Count();
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
            IEnumerable<VContractEngin> items;
            if (!String.IsNullOrEmpty(request))
            {
                switch (typeRequest)
                {
                    case "number":
                        items = _database.vContractEngins.Find(x => (list.Contains(x.Author) || list.Contains(x.Owner)) && x.Number != null && x.Number.Contains(request));
                        break;
                    case "nameObject":
                        items = _database.vContractEngins.Find(x => (list.Contains(x.Author) || list.Contains(x.Owner)) && x.NameObject != null && x.NameObject.Contains(request));
                        break;
                    case "client":
                        items = _database.vContractEngins.Find(x => (list.Contains(x.Author) || list.Contains(x.Owner)) && x.Client != null && x.Client.Contains(request));
                        break;
                    case "general":
                        items = _database.vContractEngins.Find(x => (list.Contains(x.Author) || list.Contains(x.Owner)) && x.GenContractor != null && x.GenContractor.Contains(request));
                        break;
                    default:
                        items = _database.vContractEngins.Find(x => list.Contains(x.Author) || list.Contains(x.Owner));
                        break;
                }                
            }
            else 
            { 
                items = _database.vContractEngins.Find(x => list.Contains(x.Author) || list.Contains(x.Owner));
            }
            int count = items.Count();

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