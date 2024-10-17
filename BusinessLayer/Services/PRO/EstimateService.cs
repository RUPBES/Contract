using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using BusinessLayer.Models.PRO;
using DatabaseLayer.Interfaces;
using BusinessLayer.Helpers;
using DatabaseLayer.Models.PRO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class EstimateService : IEstimateService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpHelper _httpHelper;

        public EstimateService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpHelper http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _httpHelper = http;
        }

        public int? Create(EstimateDTO item)
        {
            var user = _httpHelper.GetUserName();

            if (item is not null)
            {
                if (_database.Estimates.GetById(item.Id) is null)
                {
                    var estimate = _mapper.Map<Estimate>(item);
                    _database.Estimates.Create(estimate);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create Estimate, ID={estimate.Id}",
                            nameSpace: typeof(EstimateService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return estimate.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create Estimate, object is null",
                            nameSpace: typeof(EstimateService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            var user = _httpHelper.GetUserName();

            if (id > 0)
            {
                var emp = _database.Estimates.GetById(id);

                if (emp is not null)
                {
                    try
                    {
                        _database.Estimates.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete Estimate, ID={id}",
                            nameSpace: typeof(EstimateService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(EstimateService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete Estimate, ID is not more than zero",
                            nameSpace: typeof(EstimateService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<EstimateDTO> Find(Func<Estimate, bool> predicate)
        {
            return _mapper.Map<IEnumerable<EstimateDTO>>(_database.Estimates.Find(predicate));
        }

        public IEnumerable<EstimateDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<EstimateDTO>>(_database.Estimates.GetAll());
        }

        public EstimateDTO GetById(int id, int? secondId = null)
        {
            var estimate = _database.Estimates.GetById(id);

            if (estimate is not null)
            {
                return _mapper.Map<EstimateDTO>(estimate);
            }
            else
            {
                return null;
            }
        }

        public void Update(EstimateDTO item)
        {         
            var user = _httpHelper.GetUserName();

            if (item is not null)
            {
                item.PercentOfContrPrice = CalculatePercentOfContractPrice(item);
                item.RemainsSmrCost = (item.ContractsCost ?? 0M) - (item.DoneSmrCost ?? 0M);

                _database.Estimates.Update(_mapper.Map<Estimate>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update Estimate, ID={item.Id}",
                            nameSpace: typeof(EstimateService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update Estimate, object is null",
                            nameSpace: typeof(EstimateService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IndexViewModel GetPage(int pageSize, int pageNum, string org)
        {
            int skipEntities = (pageNum - 1) * pageSize;
            var items = _database.Estimates.GetEntityWithSkipTake(skipEntities, pageSize, org).OrderBy(x => x.BuildingCode);
            int count = items.Count();
            var t = _mapper.Map<IEnumerable<EstimateDTO>>(items);

            PageViewModel pageViewModel = new PageViewModel(count, pageNum, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = t
            };

            return viewModel;
        }

        public IndexViewModel GetPageFilter(int pageSize, int pageNum, string request, string sortOrder, string org)
        {
            var list = org.Split(',');
            int skipEntities = (pageNum - 1) * pageSize;
            IEnumerable<Estimate> items;
            if (!String.IsNullOrEmpty(request))
            {
                items = _database.Estimates
                    .FindLike("FullName", request)
                    .Where(e => list.Contains(e.Owner))
                    .ToList();
            }
            else
            {
                items = _database.Estimates.Find(e => list.Contains(e.Owner));
            }

            int count = items.Count();

            //switch (sortOrder)
            //{
            //    case "fullName":
            //        items = items.OrderBy(s => s.FullName);
            //        break;
            //    case "fullNameDesc":
            //        items = items.OrderByDescending(s => s.FullName);
            //        break;
            //    case "fio":
            //        items = items.OrderBy(s => s.Fio);
            //        break;
            //    case "fioDesc":
            //        items = items.OrderByDescending(s => s.Fio);
            //        break;
            //    case "position":
            //        items = items.OrderBy(s => s.Position);
            //        break;
            //    case "positionDesc":
            //        items = items.OrderByDescending(s => s.Position);
            //        break;
            //    case "email":
            //        items = items.OrderBy(s => s.Email);
            //        break;
            //    case "emailDesc":
            //        items = items.OrderByDescending(s => s.Email);
            //        break;
            //    default:
            //        items = items.OrderBy(s => s.Id);
            //        break;
            //}
            items = items.Skip(skipEntities).Take(pageSize);
            var t = _mapper.Map<IEnumerable<EstimateDTO>>(items);

            PageViewModel pageViewModel = new PageViewModel(count, pageNum, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = t
            };

            return viewModel;
        }

        public IndexViewModel GetPageFilterByContract(int pageSize, int pageNum, string sortOrder, int ContractId, Dictionary<string, string> SearchString, Dictionary<string, string> CurrentSearchString, Dictionary<string, List<int>> ListSearchString, Dictionary<string, List<int>> CurrentListSearchString)
        {
            if (SearchString != CurrentSearchString)
                pageNum = 1;
            int skipEntities = (pageNum - 1) * pageSize;

            List<Estimate> items = _database.Estimates.Find(x => x.ContractId == ContractId).ToList();

            #region SearchString
            string value;
            SearchString.TryGetValue("Шифр здания", out value);
            if (value != null)
                items = items.Where(x => x.BuildingCode.Contains(value)).ToList();

            SearchString.TryGetValue("Название здания", out value);     
            if (value != null)
                items = items.Where(x => x.BuildingName.Contains(value)).ToList();

            SearchString.TryGetValue("Подрядчик", out value);            
            if (value != null)
                items = items.Where(x => x.SubContractor.Contains(value)).ToList();
            
            SearchString.TryGetValue("Начало периода получения чертежа", out value);
            if (value != null)
            {
                DateTime date;
                DateTime.TryParse(value, out date);
                items = items.Where(x => x.DrawingsDate >= date).ToList();
            }
            
            SearchString.TryGetValue("Конец периода получения чертежа", out value);
            if (value != null)
            {
                DateTime date;
                DateTime.TryParse(value, out date);
                items = items.Where(x => x.DrawingsDate <= date).ToList();
            }
            
            SearchString.TryGetValue("Начало периода получения сметы", out value);
            if (value != null)
            {
                DateTime date;
                DateTime.TryParse(value, out date);
                items = items.Where(x => x.EstimateDate >= date).ToList();
            }
            
            SearchString.TryGetValue("Конец периода получения сметы", out value);
            if (value != null)
            {
                DateTime date;
                DateTime.TryParse(value, out date);
                items = items.Where(x => x.EstimateDate <= date).ToList();
            }
            #endregion

            #region ListSearchString            
            List<int> listItems;
            ListSearchString.TryGetValue("Буквенный индекс чертежей", out listItems);
            if (listItems!= null && listItems.Count > 0)
            {
                var answer = new List<Estimate>();
                foreach (var item in listItems)
                {
                    answer.AddRange(items.Where(x => x.KindOfWorkId == item));
                }
                items = answer;
            }

            ListSearchString.TryGetValue("Вид работы", out listItems);            
            if (listItems != null && listItems.Count > 0)
            {
                var answer = new List<Estimate>();
                var abbrKind = new List<AbbreviationKindOfWork>();
                foreach (var item in listItems)
                {
                    var list = _database.AbbreviationKindOfWorks.Find(x => x.KindOfWorkId == item).ToList();
                    abbrKind.AddRange(list);
                }
                foreach (var item in abbrKind)
                {
                    answer.AddRange(items.Where(x => x.KindOfWorkId == item.Id));
                }
                items = answer;
            }
            #endregion

            int count = items.Count();

            switch (sortOrder)
            {
                case "fullName":
                    items = items.OrderBy(s => s.BuildingName).ToList();
                    break;
                case "fullNameDesc":
                    items = items.OrderByDescending(s => s.BuildingName).ToList();
                    break;
                default:
                    break;
            }
            items = items.Skip(skipEntities).Take(pageSize).ToList();
            var t = _mapper.Map<IEnumerable<EstimateDTO>>(items);

            PageViewModel pageViewModel = new PageViewModel(count, pageNum, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = t
            };
            return viewModel;
        }

        public List<DatabaseLayer.Models.KDO.File> GetFiles(int EstimateId)
        {
            var filesId = _database.EstimateFiles.Find(x => x.EstimateId == EstimateId).Select(x => x.FileId).ToList();
            List<DatabaseLayer.Models.KDO.File> files = new List<DatabaseLayer.Models.KDO.File>();
            foreach (var item in filesId)
            {
                files.AddRange(_database.Files.Find(x => x.Id == item));
            }
            return files;
        }



        private decimal CalculatePercentOfContractPrice(EstimateDTO item)
        {
            var doneSmr = item.DoneSmrCost;
            var costContr = item.ContractsCost;

            if(doneSmr > 0M && costContr > 0M)
            {
                decimal devided = (decimal)((doneSmr * 100) / (costContr*100))*100;
                return Math.Round(devided, 2);
            }
            return 0M;
        }

        public Finding ReturnKeysSearch(string type)
        {

            var keyStore = new Dictionary<string, Finding>();

            keyStore.Add(ConstantsApp.SMR_PRO_APP, new Finding());
            keyStore.Add(ConstantsApp.SXW_SINKEVICH_APP, new Finding());
            keyStore.Add(ConstantsApp.BELSMETA_APP, new Finding());

            #region SMR

            keyStore[ConstantsApp.SMR_PRO_APP].Estimate = new SearchEstimateObject();
            keyStore[ConstantsApp.SMR_PRO_APP].Estimate.DocName = ConstantsApp.SMR_ESTIMATE_DOC_NAME;
            keyStore[ConstantsApp.SMR_PRO_APP].Estimate.BuildingName = ConstantsApp.SMR_ESTIMATE_BUILDING_NAME;
            keyStore[ConstantsApp.SMR_PRO_APP].Estimate.BuildingCode = ConstantsApp.SMR_ESTIMATE_BUILDING_CODE;
            keyStore[ConstantsApp.SMR_PRO_APP].Estimate.DrawingKit = ConstantsApp.SMR_ESTIMATE_DRAWING_KIT;
            keyStore[ConstantsApp.SMR_PRO_APP].Estimate.StartLineLookingForEstimateName = ConstantsApp.SMR_ESTIMATE_START_LINE_LOOKING_FOR_ESTIMATE_NAME;

            keyStore[ConstantsApp.SMR_PRO_APP].LaborCost = new SearchObject();
            keyStore[ConstantsApp.SMR_PRO_APP].LaborCost.DocName = ConstantsApp.SMR_LABOR_COST_DOC_NAME;
            keyStore[ConstantsApp.SMR_PRO_APP].LaborCost.ColName = ConstantsApp.SMR_LABOR_COST_COL_NAME;
            keyStore[ConstantsApp.SMR_PRO_APP].LaborCost.RowName = ConstantsApp.SMR_LABOR_COST_ROW_NAME;

            keyStore[ConstantsApp.SMR_PRO_APP].ContractCost = new SearchObject();
            keyStore[ConstantsApp.SMR_PRO_APP].ContractCost.DocName = ConstantsApp.SMR_CONTRACT_COST_DOC_NAME;
            keyStore[ConstantsApp.SMR_PRO_APP].ContractCost.ColName = ConstantsApp.SMR_CONTRACT_COST_COL_NAME;
            keyStore[ConstantsApp.SMR_PRO_APP].ContractCost.RowName = ConstantsApp.SMR_CONTRACT_COST_ROW_NAME;

            keyStore[ConstantsApp.SMR_PRO_APP].DoneSmrCost = new SearchObject();
            keyStore[ConstantsApp.SMR_PRO_APP].DoneSmrCost.DocName = ConstantsApp.SMR_DONE_SMR_COST_DOC_NAME;
            keyStore[ConstantsApp.SMR_PRO_APP].DoneSmrCost.ExtraColName = ConstantsApp.SMR_DONE_SMR_COST_EXTRA_COL_NAME;
            keyStore[ConstantsApp.SMR_PRO_APP].DoneSmrCost.ColName = ConstantsApp.SMR_DONE_SMR_COST_COL_NAME;
            keyStore[ConstantsApp.SMR_PRO_APP].DoneSmrCost.RowName = ConstantsApp.SMR_DONE_SMR_COST_ROW_NAME;

            #endregion

            #region SXW

            keyStore[ConstantsApp.SXW_SINKEVICH_APP].Estimate = new SearchEstimateObject();
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].Estimate.DocName = ConstantsApp.SXW_ESTIMATE_DOC_NAME;
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].Estimate.BuildingName = ConstantsApp.SXW_ESTIMATE_BUILDING_NAME;
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].Estimate.BuildingCode = ConstantsApp.SXW_ESTIMATE_BUILDING_CODE;
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].Estimate.DrawingKit = ConstantsApp.SXW_ESTIMATE_DRAWING_KIT;
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].Estimate.StartLineLookingForEstimateName = ConstantsApp.SXW_ESTIMATE_START_LINE_LOOKING_FOR_ESTIMATE_NAME;

            keyStore[ConstantsApp.SXW_SINKEVICH_APP].LaborCost = new SearchObject();
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].LaborCost.DocName = ConstantsApp.SXW_LABOR_COST_DOC_NAME;
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].LaborCost.ColName = ConstantsApp.SXW_LABOR_COST_COL_NAME;
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].LaborCost.RowName = ConstantsApp.SXW_LABOR_COST_ROW_NAME;

            keyStore[ConstantsApp.SXW_SINKEVICH_APP].ContractCost = new SearchObject();
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].ContractCost.DocName = ConstantsApp.SXW_CONTRACT_COST_DOC_NAME;
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].ContractCost.ColName = ConstantsApp.SXW_CONTRACT_COST_COL_NAME;
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].ContractCost.RowName = ConstantsApp.SXW_CONTRACT_COST_ROW_NAME;

            keyStore[ConstantsApp.SXW_SINKEVICH_APP].DoneSmrCost = new SearchObject();
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].DoneSmrCost.DocName = ConstantsApp.SXW_DONE_SMR_COST_DOC_NAME;
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].DoneSmrCost.ExtraColName = ConstantsApp.SXW_DONE_SMR_COST_EXTRA_COL_NAME;
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].DoneSmrCost.ColName = ConstantsApp.SXW_DONE_SMR_COST_COL_NAME;
            keyStore[ConstantsApp.SXW_SINKEVICH_APP].DoneSmrCost.RowName = ConstantsApp.SXW_DONE_SMR_COST_ROW_NAME;

            #endregion

            #region BELSMETA

            keyStore[ConstantsApp.BELSMETA_APP].Estimate = new SearchEstimateObject();
            keyStore[ConstantsApp.BELSMETA_APP].Estimate.DocName = ConstantsApp.BLSMT_ESTIMATE_DOC_NAME;
            keyStore[ConstantsApp.BELSMETA_APP].Estimate.BuildingName = ConstantsApp.BLSMT_ESTIMATE_BUILDING_NAME;
            keyStore[ConstantsApp.BELSMETA_APP].Estimate.BuildingCode = ConstantsApp.BLSMT_ESTIMATE_BUILDING_CODE;
            keyStore[ConstantsApp.BELSMETA_APP].Estimate.DrawingKit = ConstantsApp.BLSMT_ESTIMATE_DRAWING_KIT;
            keyStore[ConstantsApp.BELSMETA_APP].Estimate.StartLineLookingForEstimateName = ConstantsApp.BLSMT_ESTIMATE_START_LINE_LOOKING_FOR_ESTIMATE_NAME;

            keyStore[ConstantsApp.BELSMETA_APP].LaborCost = new SearchObject();
            keyStore[ConstantsApp.BELSMETA_APP].LaborCost.DocName = ConstantsApp.BLSMT_LABOR_COST_DOC_NAME;
            keyStore[ConstantsApp.BELSMETA_APP].LaborCost.ColName = ConstantsApp.BLSMT_LABOR_COST_COL_NAME;
            keyStore[ConstantsApp.BELSMETA_APP].LaborCost.RowName = ConstantsApp.BLSMT_LABOR_COST_ROW_NAME;

            keyStore[ConstantsApp.BELSMETA_APP].ContractCost = new SearchObject();
            keyStore[ConstantsApp.BELSMETA_APP].ContractCost.DocName = ConstantsApp.BLSMT_CONTRACT_COST_DOC_NAME;
            keyStore[ConstantsApp.BELSMETA_APP].ContractCost.ColName = ConstantsApp.BLSMT_CONTRACT_COST_COL_NAME;
            keyStore[ConstantsApp.BELSMETA_APP].ContractCost.RowName = ConstantsApp.BLSMT_CONTRACT_COST_ROW_NAME;

            keyStore[ConstantsApp.BELSMETA_APP].DoneSmrCost = new SearchObject();
            keyStore[ConstantsApp.BELSMETA_APP].DoneSmrCost.DocName = ConstantsApp.BLSMT_DONE_SMR_COST_DOC_NAME;
            keyStore[ConstantsApp.BELSMETA_APP].DoneSmrCost.ColName = ConstantsApp.BLSMT_DONE_SMR_COST_COL_NAME;
            keyStore[ConstantsApp.BELSMETA_APP].DoneSmrCost.RowName = ConstantsApp.BLSMT_DONE_SMR_COST_ROW_NAME;
            keyStore[ConstantsApp.BELSMETA_APP].DoneSmrCost.ExtraColName = ConstantsApp.BLSMT_DONE_SMR_COST_EXTRA_COL_NAME;
            keyStore[ConstantsApp.BELSMETA_APP].DoneSmrCost.ExtraRowName = ConstantsApp.BLSMT_DONE_SMR_COST_EXTRA_ROW_NAME;

            #endregion

            Finding? result = new Finding();
            var s = keyStore.TryGetValue(type, out result);
            return result;
        }
    }
}
