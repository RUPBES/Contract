using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Services
{
    internal class VContractEnginService : IVContractEnginService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly IContractArchiveUoW _databaseArch;
        public VContractEnginService(IContractUoW database, IMapper mapper, IContractArchiveUoW databaseArch)
        {
            _database = database;
            _mapper = mapper;
            _databaseArch = databaseArch;
        }

        public IEnumerable<VContractDTO> Find(Func<VContractEngin, bool> predicate)
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_database.vContractEngins.Find(predicate));
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

        public IndexViewModel GetPage(int pageSize, int pageNum, string org, bool useArchiveData = false)
        {
            int skipEntities = (pageNum - 1) * pageSize;
            var contractsEngin = useArchiveData ?
                    _databaseArch.vContractEngins.GetEntitySkipTake(skipEntities, pageSize, org)
                   : _database.vContractEngins.GetEntitySkipTake(skipEntities, pageSize, org);

            int count = contractsEngin.Count();
            var objIndexModel = _mapper.Map<IEnumerable<VContractDTO>>(contractsEngin);

            PageViewModel pageViewModel = new PageViewModel(count, pageNum, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = objIndexModel
            };

            return viewModel;
        }

        public IndexViewModel GetPageFilter(int pageSize, int pageNum, string request, string typeRequest, string sortOrder, string org, bool useArchiveData = false)
        {
            var list = org.Split(',');
            int skipEntities = (pageNum - 1) * pageSize;
            IEnumerable<VContractEngin> contractsView;

            if (!String.IsNullOrEmpty(request))
            {
                switch (typeRequest)
                {
                    case "number":
                        contractsView = useArchiveData ?
                                  _databaseArch.vContractEngins.FindNumberContract(request, list) :
                                  _database.vContractEngins.FindNumberContract(request, list);
                        break;
                    case "nameObject":
                        contractsView = useArchiveData ?
                                _databaseArch.vContractEngins.FindLikeNameObj(request, list) :
                                _database.vContractEngins.FindLikeNameObj(request, list);
                        break;
                    case "client":
                        contractsView = useArchiveData ?
                                _databaseArch.vContractEngins.FindOrganization(request, "client", list) :
                                _database.vContractEngins.FindOrganization(request, "client", list);
                        break;
                    case "general":
                        contractsView = useArchiveData ?
                                _databaseArch.vContractEngins.FindOrganization(request, "general", list) :
                                _database.vContractEngins.FindOrganization(request, "general", list);                        
                        break;
                    default:
                        contractsView = useArchiveData ?
                              _databaseArch.vContractEngins.Find(x => list.Contains(x.Author) || list.Contains(x.Owner)) :
                              _database.vContractEngins.Find(x => list.Contains(x.Author) || list.Contains(x.Owner));                        
                        break;
                }
            }
            else
            {
                contractsView = useArchiveData ?
                              _databaseArch.vContractEngins.Find(x => list.Contains(x.Author) || list.Contains(x.Owner)) :
                              _database.vContractEngins.Find(x => list.Contains(x.Author) || list.Contains(x.Owner));
            }
            int count = contractsView.Count();

            switch (sortOrder)
            {
                case "date":
                    contractsView = contractsView.OrderBy(s => s.Date).ThenBy(s => s.Number);
                    break;
                case "dateDesc":
                    contractsView = contractsView.OrderByDescending(s => s.Date).ThenBy(s => s.Number);
                    break;
                case "nameObject":
                    contractsView = contractsView.OrderBy(s => s.NameObject).ThenBy(s => s.Id);
                    break;
                case "nameObjectDesc":
                    contractsView = contractsView.OrderByDescending(s => s.NameObject).ThenBy(s => s.Id);
                    break;
                case "client":
                    contractsView = contractsView.OrderBy(s => s.Client).ThenBy(s => s.Id);
                    break;
                case "clientDesc":
                    contractsView = contractsView.OrderByDescending(s => s.Client).ThenBy(s => s.Id);
                    break;
                case "genContractor":
                    contractsView = contractsView.OrderBy(s => s.GenContractor).ThenBy(s => s.Id);
                    break;
                case "genContractorDesc":
                    contractsView = contractsView.OrderByDescending(s => s.GenContractor).ThenBy(s => s.Id);
                    break;
                case "dateEnter":
                    contractsView = contractsView.OrderBy(s => s.EnteringTerm).ThenBy(s => s.Id);
                    break;
                case "dateEnterDesc":
                    contractsView = contractsView.OrderByDescending(s => s.EnteringTerm).ThenBy(s => s.Id);
                    break;
                default:
                    contractsView = contractsView.OrderBy(s => s.Id);
                    break;
            }

            contractsView.Skip(skipEntities).Take(pageSize);
            var t = _mapper.Map<IEnumerable<VContractDTO>>(contractsView);

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