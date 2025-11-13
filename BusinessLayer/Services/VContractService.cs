using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Interfaces.Entities;
using DatabaseLayer.Models.KDO;
using System.Diagnostics;

namespace BusinessLayer.Services
{
    internal class VContractService : IVContractService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly IContractArchiveUoW _databaseArch;
        private readonly IReadonlyRepoDapper<VContract> _databaseDp;
        private readonly IReadonlyContractDapperRepo _databaseContractDp;
        public VContractService(IContractUoW database, IMapper mapper, IContractArchiveUoW databaseArch, 
            IReadonlyRepoDapper<VContract> databaseDp, IReadonlyContractDapperRepo databaseContractDp)
        {
            _database = database;
            _mapper = mapper;
            _databaseArch = databaseArch;
            _databaseDp = databaseDp;
            _databaseContractDp = databaseContractDp;
        }

        public IEnumerable<VContractDTO> Find(Func<VContract, bool> predicate)
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_database.vContracts.Find(predicate));
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
            var contract = _databaseContractDp.GetById($"where c.Id ={id}");

            if (contract is not null)
            {
                return _mapper.Map<VContractDTO>(contract);
            }
            return null;

        }

        public IndexViewModel GetPage(int pageSize, int pageNum, string org, bool useArchiveData)
        {
            int skipEntities = (pageNum - 1) * pageSize;

            var items = useArchiveData ?
                     _databaseArch.vContracts.GetEntitySkipTake(skipEntities, pageSize, org)
                    : _databaseDp.GetEntitySkipTake(skipEntities, pageSize, org);

            int count = useArchiveData ?
               _databaseArch.vContracts.Count() :
               _databaseDp.Count(org.Split(','));


            var objIndexModel = _mapper.Map<IEnumerable<VContractDTO>>(items);

            PageViewModel pageViewModel = new PageViewModel(count, pageNum, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = objIndexModel
            };
            return viewModel;
        }

        public IndexViewModel GetPageFilter(int pageSize, int pageNum, string request, string typeRequest, string sortOrder, string org, bool useArchiveData)
        {
            var orgList = org.Split(',');
            int skipEntities = (pageNum - 1) * pageSize;
            IEnumerable<VContract> contractsView;

            if (!String.IsNullOrEmpty(request))
            {
                switch (typeRequest)
                {
                    case "number":
                        contractsView = useArchiveData ?
                                  _databaseArch.vContracts.FindNumberContract(request, orgList) :
                                  _databaseDp.Find($" and c.Number like('%{request}%') ORDER BY Date DESC", orgList);
                        //_database.vContracts.FindNumberContract(request, listOrganization);

                        break;
                    case "nameObject":
                        contractsView = useArchiveData ?
                                 _databaseArch.vContracts.FindLikeNameObj(request, orgList) :
                                 _databaseDp.Find($"and c.NameObject like('%{request}%') ORDER BY Date DESC", orgList);
                        //_database.vContracts.FindLikeNameObj(request, listOrganization);
                        break;
                    case "client":
                        contractsView = useArchiveData ?
                                 _databaseArch.vContracts.FindOrganization(request, "client", orgList) :
                                 _databaseDp.Find($"and c.Client like('%{request}%') ORDER BY Date DESC", orgList);
                        //_database.vContracts.FindOrganization(request, "client", listOrganization);

                        break;
                    case "general":
                        contractsView = useArchiveData ?
                                _databaseArch.vContracts.FindOrganization(request, "general", orgList) :
                                _databaseDp.Find($"and c.GenContractor like('%{request}%') ORDER BY Date DESC", orgList);
                        //_database.vContracts.FindOrganization(request, "general", listOrganization);
                        break;
                    default:
                        contractsView = useArchiveData ?
                               _databaseArch.vContracts.Find(x => orgList.Contains(x.Owner)) :
                               _databaseDp.Find($"ORDER BY Date DESC", orgList);
                        //_database.vContracts.Find(x => orgList.Contains(x.Owner));
                        break;
                }
            }
            else
            {
                contractsView = useArchiveData ?
                    _databaseArch.vContracts.Find(x => orgList.Contains(x.Owner))
                    : _databaseDp.Find($"ORDER BY Date DESC", orgList);
                //: _database.vContracts.Find(x => orgList.Contains(x.Owner));
            }

            int count = useArchiveData ?
               _databaseArch.vContracts.Count() :
                _databaseDp.Count(orgList);
            //_database.vContracts.Count();

            switch (sortOrder)
            {
                case "number":
                    contractsView = contractsView.OrderBy(s => s.Date).ThenBy(s => s.Number);
                    break;
                case "numberDesc":
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

            contractsView = contractsView.Skip(skipEntities).Take(pageSize);

            foreach (var item in contractsView)
            {
                var amend = useArchiveData ?
                    _databaseArch.Amendments
                    .Find(x => x.ContractId == item.Id).OrderBy(x => x.Date)
                    .Select(x => new { DateBeginWork = x.DateBeginWork, DateEndWork = x.DateEndWork, DateEntryObject = x.DateEntryObject }).LastOrDefault()

                    : _database.Amendments
                    .Find(x => x.ContractId == item.Id).OrderBy(x => x.Date)
                    .Select(x => new { DateBeginWork = x.DateBeginWork, DateEndWork = x.DateEndWork, DateEntryObject = x.DateEntryObject }).LastOrDefault();

                if (amend is not null)
                {
                    item.DateBeginWork = amend.DateBeginWork;
                    item.DateEndWork = amend.DateEndWork;
                    item.EnteringTerm = amend.DateEntryObject;
                }
            }
            var objIndexModel = _mapper.Map<IEnumerable<VContractDTO>>(contractsView);

            PageViewModel pageViewModel = new PageViewModel(count, pageNum, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = objIndexModel
            };

            return viewModel;
        }

        /// <summary>
        /// Возвращает список договоров, принадлежащих генподрядному договору по его ID и 
        /// типу необходимых договоров
        /// </summary>
        /// <param name="id">ID Гендоговора</param>
        /// <param name="contractType">Тип договора, который необходимо найти (Соглашение, субподряд, подобъект))</param>
        /// <returns>список вложенных договоров принадлежащих генподрядному</returns>
        public IEnumerable<VContractDTO> GetSubsByType(int? id, Enums.Contract? contractType, bool useArchiveData)
        {
            if (!id.HasValue || contractType == null)
            {
                return Enumerable.Empty<VContractDTO>();
            }

            Func<DatabaseLayer.Models.KDO.Contract, bool> selector;
            string sqlPredicate = string.Empty;

            if (contractType == Enums.Contract.SubContract)
            {
                sqlPredicate = $"where c.SubContractId = @id and c.IsSubContract = 1  ORDER BY Date DESC";
            }
            else if (contractType == Enums.Contract.Agreement)
            {
                sqlPredicate = $"where c.AgreementContractId = @id and c.IsAgreementContract = 1  ORDER BY Date DESC";
            }
            else if (contractType == Enums.Contract.MultipleContract)
            {
                sqlPredicate = $"where c.MultipleContractId = @id and c.IsOneOfMultiple = 1  ORDER BY Date DESC";
            }
            else
            {
                return Enumerable.Empty<VContractDTO>();
            }

            //var s = _databaseDp.Find($"where c.Number like('%{request}%') and c.Owner IN @orgList  ORDER BY Date DESC");

            //var contracts = useArchiveData ?
            //    _databaseArch.Contracts.Find(selector) :
            //    _database.Contracts.Find(selector);
            var contracts = _databaseContractDp.GetSubsById(id.Value, sqlPredicate);
            if (contracts.Any())
            {
                //foreach (var item in contracts)
                //{
                //    var amend = useArchiveData ?
                //                _databaseArch.Amendments.Find(x => x.ContractId == item.Id).ToList() :
                //                _database.Amendments.Find(x => x.ContractId == item.Id).ToList();

                //    if (amend.Count > 0)
                //    {
                //        amend = amend.OrderBy(x => x.Date).ToList();
                //        item.ContractPrice = amend.Last().ContractPrice;
                //    }
                //}
                return _mapper.Map<IEnumerable<VContractDTO>>(contracts);
            }
            else
            {
                return Enumerable.Empty<VContractDTO>();
            }
        }


    }
}
