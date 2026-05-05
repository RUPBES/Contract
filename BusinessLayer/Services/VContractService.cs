using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models.KDO;
using BusinessLayer.Models.Settings;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Interfaces.Dapper;
using DatabaseLayer.Models.KDO;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace BusinessLayer.Services
{
    internal class VContractService : IVContractService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly DbSettings _archiveOptions;
        private readonly IReadonlyRepoDapper<VContract> _vContractDpr;
        private readonly IReadonlyContractDapperRepo _contractDpr;
        public VContractService(IContractUoW database, IMapper mapper, IReadonlyRepoDapper<VContract> databaseDp,
            IReadonlyContractDapperRepo databaseContractDp, IOptions<DbSettings> archiveOptions)
        {
            _database = database;
            _mapper = mapper;
            _vContractDpr = databaseDp;
            _contractDpr = databaseContractDp;
            _archiveOptions = archiveOptions.Value;
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
            var contract = _contractDpr.GetById($"where c.Id ={id}");

            if (contract is not null)
            {
                return _mapper.Map<VContractDTO>(contract);
            }
            return null;
        }

        public IndexViewModel GetPage(int pageSize, int pageNum, string org, bool useArchiveData)
        {
            int skipEntities = (pageNum - 1) * pageSize;

            var items = _vContractDpr.GetEntitySkipTake(skipEntities, pageSize, org, useArchiveData ? _archiveOptions.TargetArchiveDb : null);
            int count = _vContractDpr.Count(org.Split(','), useArchiveData ? _archiveOptions.TargetArchiveDb : null);
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
                        contractsView = _vContractDpr.Find($" and c.Number like('%{request}%') ORDER BY Date DESC",
                                                        orgList,
                                                        useArchiveData ? _archiveOptions.TargetArchiveDb : null);
                        break;
                    case "nameObject":
                        contractsView = _vContractDpr.Find($" and c.NameObject like('%{request}%') ORDER BY Date DESC",
                                                    orgList,
                                                    useArchiveData ? _archiveOptions.TargetArchiveDb : null);
                        break;
                    case "client":
                        contractsView = _vContractDpr.Find($" and c.Client like('%{request}%') ORDER BY Date DESC",
                                                 orgList,
                                                 useArchiveData ? _archiveOptions.TargetArchiveDb : null);
                        break;
                    case "general":
                        contractsView = _vContractDpr.Find($" and c.GenContractor like('%{request}%') ORDER BY Date DESC"
                                 , orgList
                                 , useArchiveData ? _archiveOptions.TargetArchiveDb : null);
                        break;
                    default:
                        contractsView = _vContractDpr.Find($"ORDER BY Date DESC", orgList, useArchiveData ? _archiveOptions.TargetArchiveDb : null);
                        break;
                }
            }
            else
            {
                contractsView = _vContractDpr.Find($"ORDER BY Date DESC", orgList, useArchiveData ? _archiveOptions.TargetArchiveDb : null);
            }

            int count = _vContractDpr.Count(orgList, useArchiveData ? _archiveOptions.TargetArchiveDb : null);

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
        public IEnumerable<VContractDTO> GetSubsByType(int? id, Enums.ContractType? contractType, bool useArchiveData)
        {
            if (!id.HasValue || contractType == null)
            {
                return Enumerable.Empty<VContractDTO>();
            }

            Func<Contract, bool> selector;
            string sqlPredicate = string.Empty;

            if (contractType == Enums.ContractType.SubContract)
            {
                sqlPredicate = $"where c.SubContractId = @id and c.IsSubContract = 1  ORDER BY Date DESC";
            }
            else if (contractType == Enums.ContractType.Agreement)
            {
                sqlPredicate = $"where c.AgreementContractId = @id and c.IsAgreementContract = 1  ORDER BY Date DESC";
            }
            else if (contractType == Enums.ContractType.MultipleContract)
            {
                sqlPredicate = $"where c.MultipleContractId = @id and c.IsOneOfMultiple = 1  ORDER BY Date DESC";
            }
            else
            {
                return Enumerable.Empty<VContractDTO>();
            }

            var contracts = _contractDpr.GetSubsById(id.Value, sqlPredicate);
            if (contracts.Any())
            {
                return _mapper.Map<IEnumerable<VContractDTO>>(contracts);
            }
            else
            {
                return Enumerable.Empty<VContractDTO>();
            }
        }

        /// <summary>
        /// Возвращает список договоров, принадлежащих генподрядному договору по его ID и 
        /// типу необходимых договоров
        /// </summary>
        /// <param name="id">ID Гендоговора</param>
        /// <param name="contractType">Тип договора, который необходимо найти (Соглашение, субподряд, подобъект))</param>
        /// <returns>список вложенных договоров принадлежащих генподрядному</returns>
        public (IEnumerable<VContractDTO> genClientContracts, IEnumerable<VContractDTO> subContracts) GetByOrganizationId(int orgId, bool? useArchiveData = null)
        {
            (IEnumerable<VContractDTO> genClientContracts, IEnumerable<VContractDTO> subContracts) contracts = (Enumerable.Empty<VContractDTO>(), Enumerable.Empty<VContractDTO>());

            if (orgId < 1)
            {
                return contracts;
            }

            var contractsDb = _contractDpr.GetByOrganizationId(orgId, (useArchiveData is true ? _archiveOptions.TargetArchiveDb : null));
            contracts.genClientContracts = _mapper.Map<IEnumerable<VContractDTO>>(contractsDb.genClientContracts);
            contracts.subContracts = _mapper.Map<IEnumerable<VContractDTO>>(contractsDb.subContracts);
            return contracts;
        }

        public IndexViewModel Filter(int pageSize, int pageNum, string type, string? sortDirection, string org, string? searchText, string? whereCondition, bool? useArchiveData)
        {
            int skipEntities = (pageNum - 1) * pageSize;
            (IEnumerable<VContract>, int) contractsView;

            switch (type)
            {
                case "date":
                    contractsView = _vContractDpr.Filter(
                        skipEntities,
                        pageSize,
                        org,
                        searchText is null ? "" : $@" and Date LIKE ('%{searchText}%')",
                         $@" ORDER BY Date {sortDirection} ",
                        (useArchiveData is true ? _archiveOptions.TargetArchiveDb : null));
                    break;
                case "number":
                    contractsView = _vContractDpr.Filter(
                        skipEntities,
                        pageSize,
                        org,
                         GetWhereCondition("Number", searchText, whereCondition),
                         $@" ORDER BY Number {sortDirection} ",
                        (useArchiveData is true ? _archiveOptions.TargetArchiveDb : null));
                    break;
                case "nameObject":
                    contractsView = _vContractDpr.Filter(
                         skipEntities,
                         pageSize,
                         org,
                         GetWhereCondition("NameObject", searchText, whereCondition),
                          $@" ORDER BY NameObject {sortDirection} ",
                         (useArchiveData is true ? _archiveOptions.TargetArchiveDb : null));
                    break;
                case "client":
                    contractsView = _vContractDpr.Filter(
                        skipEntities,
                        pageSize,
                        org,
                        GetWhereCondition("Client", searchText, whereCondition),
                         $@" ORDER BY Client {sortDirection} ",
                        (useArchiveData is true ? _archiveOptions.TargetArchiveDb : null));
                    break;
                case "gencontractor":
                    contractsView = _vContractDpr.Filter(
                         skipEntities,
                         pageSize,
                         org,
                         GetWhereCondition("GenContractor", searchText, whereCondition),
                          $@" ORDER BY GenContractor {sortDirection} ",
                         (useArchiveData is true ? _archiveOptions.TargetArchiveDb : null));
                    break;
                case "enteringTerm":
                    contractsView = _vContractDpr.Filter(
                         skipEntities,
                         pageSize,
                         org,
                         GetWhereCondition("EnteringTerm", searchText, whereCondition),
                         $@" ORDER BY EnteringTerm {sortDirection} ",
                         (useArchiveData is true ? _archiveOptions.TargetArchiveDb : null));
                    break;
                default:
                    contractsView = _vContractDpr.Filter(
                        skipEntities,
                        pageSize,
                        org,
                        !string.IsNullOrEmpty(whereCondition) ? $" and {whereCondition} " : "",
                        $@" ORDER BY Id {sortDirection} ",
                        (useArchiveData is true ? _archiveOptions.TargetArchiveDb : null));
                    break;
            }

            var objIndexModel = _mapper.Map<IEnumerable<VContractDTO>>(contractsView.Item1);

            PageViewModel pageViewModel = new PageViewModel(contractsView.Item2, pageNum, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = objIndexModel
            };

            return viewModel;
        }

        private string GetWhereCondition(string columnName, string? text, string? whereClause)
        {
            if (string.IsNullOrEmpty(whereClause) && !string.IsNullOrEmpty(text))
            {
                return $" and ( {columnName} LIKE ('%{text}%')) ";
            }

            if (!string.IsNullOrEmpty(whereClause) && string.IsNullOrEmpty(text))
            {
                return $" and {whereClause} ";
            }

            if (!string.IsNullOrEmpty(whereClause) && !string.IsNullOrEmpty(text))
            {
                return $" and ( {columnName} LIKE ('%{text}%')) and {whereClause}";
            }

            return string.Empty;
        }

    }
}