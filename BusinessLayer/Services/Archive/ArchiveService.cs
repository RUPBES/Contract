using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Services.Archive
{
    internal class ArchiveService : IArchiveService
    {
        private IMapper _mapper;
        private readonly IContractArchiveUoW _databaseArch;
        public ArchiveService(IMapper mapper, IContractArchiveUoW databaseArch)
        {
            _mapper = mapper;
            _databaseArch = databaseArch;
        }

        public IEnumerable<VContractDTO> FindVContract(Func<VContract, bool> predicate)
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_databaseArch.vContracts.Find(predicate));
        }



        public IEnumerable<VContractDTO> FindContractByQuery(string queryString)
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_databaseArch.vContracts.FindContract(queryString));
        }

        public IEnumerable<VContractDTO> FindLikeNameObj(string queryString)
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_databaseArch.vContracts.FindLikeNameObj(queryString));
        }

        public IEnumerable<VContractDTO> GetAllContracts()
        {
            return _mapper.Map<IEnumerable<VContractDTO>>(_databaseArch.vContracts.GetAll());
        }

        public ContractDTO GetContractById(int id)
        {
            var contract = _databaseArch.Contracts.GetById(id);

            if (contract is not null)
            {
                return _mapper.Map<ContractDTO>(contract);
            }
            else
            {
                return null;
            }
        }

        public IndexViewModel GetContractPage(int pageSize, int pageNum, string org)
        {
            int skipEntities = (pageNum - 1) * pageSize;

            var items = _databaseArch.vContracts.GetEntitySkipTake(skipEntities, pageSize, org);

            int count = items.Count();
            foreach (var item in items)
            {
                var amend = _databaseArch.Amendments.Find(x => x.ContractId == item.Id).OrderBy(x => x.Date).
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

        public IndexViewModel GetContractPageFilter(int pageSize, int pageNum, string request, string typeRequest, string sortOrder, string org)
        {
            var list = org.Split(',');
            int skipEntities = (pageNum - 1) * pageSize;
            IEnumerable<VContract> items;
            if (!String.IsNullOrEmpty(request))
            {
                switch (typeRequest)
                {
                    case "number":
                        items = _databaseArch.vContracts.FindNumberContract(request, list);
                        break;
                    case "nameObject":
                        items = _databaseArch.vContracts.FindLikeNameObj(request, list);
                        break;
                    case "client":
                        items = _databaseArch.vContracts.FindOrganization(request, "client", list);// _databaseArch.vContracts.Find(x => list.Contains(x.Owner) && x.Client != null && x.Client.Contains(request));
                        break;
                    case "general":
                        items = _databaseArch.vContracts.FindOrganization(request, "general", list);
                        break;
                    default:
                        items = _databaseArch.vContracts.Find(x => list.Contains(x.Owner));
                        break;
                }
            }
            else
            {
                items = _databaseArch.vContracts.Find(x => list.Contains(x.Owner));
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
                var amend = _databaseArch.Amendments.Find(x => x.ContractId == item.Id).OrderBy(x => x.Date).
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

        /// <summary>
        /// Возвращает список договоров, принадлежащих генподрядному договору по его ID и 
        /// типу необходимых договоров
        /// </summary>
        /// <param name="id">ID Гендоговора</param>
        /// <param name="contractType">Тип договора, который необходимо найти (Соглашение, субподряд, подобъект))</param>
        /// <returns>список вложенных договоров принадлежащих генподрядному</returns>
        public IEnumerable<ContractDTO> GetSubsByType(int? id, ContractType? contractType)
        {
            if (!id.HasValue || contractType == null)
            {
                return Enumerable.Empty<ContractDTO>();
            }
            Func<Contract, bool> selector;

            if (contractType == ContractType.SubContract)
            {
                selector = x => x.SubContractId == id && x.IsSubContract == true;
            }
            else if (contractType == ContractType.Agreement)
            {
                selector = x => x.AgreementContractId == id && x.IsAgreementContract == true;
            }
            else if (contractType == ContractType.MultipleContract)
            {
                selector = x => x.MultipleContractId == id && x.IsOneOfMultiple == true;
            }
            else
            {
                return Enumerable.Empty<ContractDTO>();
            }

            var contracts = _databaseArch.Contracts.Find(selector);

            if (contracts.Any())
            {
                foreach (var item in contracts)
                {
                    var amend = _databaseArch.Amendments.Find(x => x.ContractId == item.Id).ToList();
                    if (amend.Count > 0)
                    {
                        amend = amend.OrderBy(x => x.Date).ToList();
                        item.ContractPrice = amend.Last().ContractPrice;
                    }
                }
                return _mapper.Map<IEnumerable<ContractDTO>>(contracts);
            }
            else
            {
                return Enumerable.Empty<ContractDTO>();
            }
        }


        public TypeWorkDTO GetTypeWorkByContractId(int contractId)
        {
            var typeWorkId = _databaseArch.TypeWorkContracts.Find(x => x.ContractId == contractId)?.FirstOrDefault()?.TypeWorkId;
            if (typeWorkId is null)
            {
                return null;
            }
            var typeWork = _databaseArch.TypeWorks.GetById((int)typeWorkId);
            if (typeWork is not null)
            {
                return _mapper.Map<TypeWorkDTO>(typeWork);
            }
            else
            {
                return null;
            }
        }


        public OrganizationDTO FindByContractOrganization(Func<ContractOrganization, bool> predicate)
        {
            var orgId = _databaseArch.ContractOrganizations?.Find(predicate)?.FirstOrDefault()?.OrganizationId;
            if (orgId is not null)
            {
                return _mapper.Map<OrganizationDTO>(_databaseArch.Organizations.GetById((int)orgId));
            }
            else
            {
                return null;
            }
        }

        public EmployeeDTO FindByContractEmployee(Func<EmployeeContract, bool> predicate)
        {
            var emplId = _databaseArch.EmployeeContracts?.Find(predicate)?.FirstOrDefault()?.EmployeeId;
            if (emplId is not null)
            {
                return _mapper.Map<EmployeeDTO>(_databaseArch.Employees.GetById((int)emplId));
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<SelectionProcedureDTO> FindSlctProcedure(Func<SelectionProcedure, bool> predicate)
        {
            return _mapper.Map<IEnumerable<SelectionProcedureDTO>>(_databaseArch.SelectionProcedures.Find(predicate));
        }
    }
}

