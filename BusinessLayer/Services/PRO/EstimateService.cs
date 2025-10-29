using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using BusinessLayer.Models.PRO;
using DatabaseLayer.Interfaces;
using BusinessLayer.Helpers;
using DatabaseLayer.Models.PRO;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;

namespace BusinessLayer.Services
{
    internal class EstimateService : IEstimateService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly IContractArchiveUoW _databaseArch;
        private readonly ILoggerContract _logger;
        private readonly IHostingEnvironment _env;

        public EstimateService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHostingEnvironment env, IContractArchiveUoW databaseArch)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _env = env;
            _databaseArch = databaseArch;
        }

        public int? Create(EstimateDTO item)
        {
            if (item is not null && !string.IsNullOrEmpty(item.Number))
            {
                if (_database.Estimates.GetById(item.Id) is null && _database.Estimates.Find(x => x.FullNumber == item.FullNumber && x.ContractId == item.ContractId)?.FirstOrDefault() is null)
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
            else
            {
                _logger.WriteLog(
                logLevel: LogLevel.Information,
                message: $"not create Estimate, object is null or number is empty",
                nameSpace: typeof(EstimateService).Name,
                methodName: MethodBase.GetCurrentMethod().Name);

            }

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var estimate = _database.Estimates.GetById(id);

                if (estimate is not null)
                {
                    try
                    {
                        ///удаляем файлы в папке и в базе, если они есть
                        var files = _database.EstimateFiles.Find(x => x.EstimateId == id);
                        if (files?.Count() > 0)
                        {
                            foreach (var item in files)
                            {
                                var filePath = _env.WebRootPath + item.File.FilePath;
                                //todo: 1) предусмотреть удаление папок с файлами
                                if (File.Exists(filePath))
                                {
                                    File.Delete(filePath);

                                    _logger.WriteLog(logLevel: LogLevel.Information, message: $"file has been removed from folder {filePath}",
                                       nameSpace: typeof(FileService).Name, methodName: MethodBase.GetCurrentMethod().Name);
                                }
                                _database.Files.Delete(item.FileId);
                            }
                        }

                        /// проверка на наличие изменени смет связанных с данной сметой
                        /// 
                        if (estimate?.ChangeEstimateId != null && estimate?.ChangeEstimateId > 0)
                        {
                            if (estimate.IsChange != true)
                            {
                                var parentEstimate = _database.Estimates.GetById((int)estimate.ChangeEstimateId);
                                parentEstimate.IsChange = false;
                            }
                            else
                            {
                                var childEstimate = _database.Estimates.Find(x => x.ChangeEstimateId == estimate.Id).FirstOrDefault();
                                if (childEstimate is not null)
                                {
                                    childEstimate.ChangeEstimateId = estimate.ChangeEstimateId;
                                }
                            }
                        }

                        _database.Estimates.Delete(id);
                        _database.Save();

                        _logger.WriteLog(logLevel: LogLevel.Information, message: $"delete Estimate, ID={id}",
                            nameSpace: typeof(EstimateService).Name, methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(logLevel: LogLevel.Error, message: e.Message,
                            nameSpace: typeof(EstimateService).Name, methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(logLevel: LogLevel.Warning, message: $"not delete Estimate, ID is not more than zero",
                            nameSpace: typeof(EstimateService).Name, methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<EstimateDTO> Find(Func<Estimate, bool> predicate, bool? useArchiveData)
        {
            return useArchiveData == true? 
                _mapper.Map<IEnumerable<EstimateDTO>>(_databaseArch.Estimates.Find(predicate)):
                _mapper.Map<IEnumerable<EstimateDTO>>(_database.Estimates.Find(predicate));
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
            if (item is not null)
            {
                item.PercentOfContrPrice = CalculatePercentOfContractPrice(item);
                item.RemainsSmrCost = (item.ContractsCost ?? 0M) - (item.DoneSmrCost ?? 0M);

                _database.Estimates.Update(_mapper.Map<Estimate>(item));
                _database.Save();

                _logger.WriteLog(logLevel: LogLevel.Information,
                                message: $"update Estimate, ID={item.Id}",
                                nameSpace: typeof(EstimateService).Name,
                                methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(logLevel: LogLevel.Warning,
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

            PageViewModel pageViewModel = new(count, pageNum, pageSize);
            IndexViewModel viewModel = new()
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

        public IndexViewModel GetPageFilterByContract(int pageSize, int pageNum, string sortOrder, int ContractId, Dictionary<string, string> SearchString, Dictionary<string, string> CurrentSearchString, Dictionary<string, List<int>> ListSearchString, Dictionary<string, List<int>> CurrentListSearchString, bool? useArchiveData)
        {
            if (SearchString != CurrentSearchString)
                pageNum = 1;
            int skipEntities = (pageNum - 1) * pageSize;

            List<Estimate> items = (useArchiveData == true)?
                _databaseArch.Estimates.Find(x => x.ContractId == ContractId).ToList():
                _database.Estimates.Find(x => x.ContractId == ContractId).ToList();

            #region SearchString
            string value;
            SearchString.TryGetValue("Шифр здания", out value);
            if (value != null)
                items = items.Where(x => x.BuildingCode != null && x.BuildingCode.ToLower().Contains(value.ToLower())).ToList();

            SearchString.TryGetValue("Название здания", out value);
            if (value != null)
                items = items.Where(x => x.BuildingName != null && x.BuildingName.ToLower().Contains(value.ToLower())).ToList();

            SearchString.TryGetValue("Подрядчик", out value);
            if (value != null)
                items = items.Where(x => x.SubContractor != null && x.SubContractor.ToLower().Contains(value.ToLower())).ToList();

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
            if (listItems != null && listItems.Count > 0)
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
                    var list = (useArchiveData == true)?
                        _databaseArch.AbbreviationKindOfWorks.Find(x => x.KindOfWorkId == item).ToList():
                        _database.AbbreviationKindOfWorks.Find(x => x.KindOfWorkId == item).ToList();

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

            if (doneSmr > 0M && costContr > 0M)
            {
                decimal devided = (decimal)((doneSmr * 100) / (costContr * 100)) * 100;
                return Math.Round(devided, 2);
            }
            return 0M;
        }

        public Finding ReturnKeysSearch(string type)
        {
            var keyStore = new Dictionary<string, Finding>();
            #region SMR

            if (type.Equals(Constants.SMR_PRO_APP, StringComparison.OrdinalIgnoreCase))
            {
                keyStore.Add(Constants.SMR_PRO_APP, new Finding());
                keyStore[Constants.SMR_PRO_APP].Estimate = new SearchEstimateObject();
                keyStore[Constants.SMR_PRO_APP].Estimate.DocName = Constants.SMR_ESTIMATE_DOC_NAME;
                keyStore[Constants.SMR_PRO_APP].Estimate.BuildingName = Constants.SMR_ESTIMATE_BUILDING_NAME;
                keyStore[Constants.SMR_PRO_APP].Estimate.BuildingCode = Constants.SMR_ESTIMATE_BUILDING_CODE;
                keyStore[Constants.SMR_PRO_APP].Estimate.DrawingKit = Constants.SMR_ESTIMATE_DRAWING_KIT;
                keyStore[Constants.SMR_PRO_APP].Estimate.StartLineLookingForEstimateName = Constants.SMR_ESTIMATE_START_LINE_LOOKING_FOR_ESTIMATE_NAME;

                keyStore[Constants.SMR_PRO_APP].LaborCost = new SearchObject();
                keyStore[Constants.SMR_PRO_APP].LaborCost.DocName = Constants.SMR_LABOR_COST_DOC_NAME;
                keyStore[Constants.SMR_PRO_APP].LaborCost.ColName = Constants.SMR_LABOR_COST_COL_NAME;
                keyStore[Constants.SMR_PRO_APP].LaborCost.RowName = Constants.SMR_LABOR_COST_ROW_NAME;

                keyStore[Constants.SMR_PRO_APP].ContractCost = new SearchObject();
                keyStore[Constants.SMR_PRO_APP].ContractCost.DocName = Constants.SMR_CONTRACT_COST_DOC_NAME;
                keyStore[Constants.SMR_PRO_APP].ContractCost.ColName = Constants.SMR_CONTRACT_COST_COL_NAME;
                keyStore[Constants.SMR_PRO_APP].ContractCost.RowName = Constants.SMR_CONTRACT_COST_ROW_NAME;

                keyStore[Constants.SMR_PRO_APP].DoneSmrCost = new SearchObject();
                keyStore[Constants.SMR_PRO_APP].DoneSmrCost.DocName = Constants.SMR_DONE_SMR_COST_DOC_NAME;
                keyStore[Constants.SMR_PRO_APP].DoneSmrCost.ExtraColName = Constants.SMR_DONE_SMR_COST_EXTRA_COL_NAME;
                keyStore[Constants.SMR_PRO_APP].DoneSmrCost.ColName = Constants.SMR_DONE_SMR_COST_COL_NAME;
                keyStore[Constants.SMR_PRO_APP].DoneSmrCost.RowName = Constants.SMR_DONE_SMR_COST_ROW_NAME;
            }
            #endregion

            #region SXW
            else if (type.Equals(Constants.SXW_SINKEVICH_APP, StringComparison.OrdinalIgnoreCase))
            {
                keyStore.Add(Constants.SXW_SINKEVICH_APP, new Finding());
                keyStore[Constants.SXW_SINKEVICH_APP].Estimate = new SearchEstimateObject();
                keyStore[Constants.SXW_SINKEVICH_APP].Estimate.DocName = Constants.SXW_ESTIMATE_DOC_NAME;
                keyStore[Constants.SXW_SINKEVICH_APP].Estimate.BuildingName = Constants.SXW_ESTIMATE_BUILDING_NAME;
                keyStore[Constants.SXW_SINKEVICH_APP].Estimate.BuildingCode = Constants.SXW_ESTIMATE_BUILDING_CODE;
                keyStore[Constants.SXW_SINKEVICH_APP].Estimate.DrawingKit = Constants.SXW_ESTIMATE_DRAWING_KIT;
                keyStore[Constants.SXW_SINKEVICH_APP].Estimate.StartLineLookingForEstimateName = Constants.SXW_ESTIMATE_START_LINE_LOOKING_FOR_ESTIMATE_NAME;

                keyStore[Constants.SXW_SINKEVICH_APP].LaborCost = new SearchObject();
                keyStore[Constants.SXW_SINKEVICH_APP].LaborCost.DocName = Constants.SXW_LABOR_COST_DOC_NAME;
                keyStore[Constants.SXW_SINKEVICH_APP].LaborCost.ColName = Constants.SXW_LABOR_COST_COL_NAME;
                keyStore[Constants.SXW_SINKEVICH_APP].LaborCost.RowName = Constants.SXW_LABOR_COST_ROW_NAME;

                keyStore[Constants.SXW_SINKEVICH_APP].ContractCost = new SearchObject();
                keyStore[Constants.SXW_SINKEVICH_APP].ContractCost.DocName = Constants.SXW_CONTRACT_COST_DOC_NAME;
                keyStore[Constants.SXW_SINKEVICH_APP].ContractCost.ColName = Constants.SXW_CONTRACT_COST_COL_NAME;
                keyStore[Constants.SXW_SINKEVICH_APP].ContractCost.RowName = Constants.SXW_CONTRACT_COST_ROW_NAME;

                keyStore[Constants.SXW_SINKEVICH_APP].DoneSmrCost = new SearchObject();
                keyStore[Constants.SXW_SINKEVICH_APP].DoneSmrCost.DocName = Constants.SXW_DONE_SMR_COST_DOC_NAME;
                keyStore[Constants.SXW_SINKEVICH_APP].DoneSmrCost.ExtraColName = Constants.SXW_DONE_SMR_COST_EXTRA_COL_NAME;
                keyStore[Constants.SXW_SINKEVICH_APP].DoneSmrCost.ColName = Constants.SXW_DONE_SMR_COST_COL_NAME;
                keyStore[Constants.SXW_SINKEVICH_APP].DoneSmrCost.RowName = Constants.SXW_DONE_SMR_COST_ROW_NAME;
            }
            #endregion

            #region BELSMETA

            else if (type.Equals(Constants.BELSMETA_APP, StringComparison.OrdinalIgnoreCase))
            {
                keyStore.Add(Constants.BELSMETA_APP, new Finding());
                keyStore[Constants.BELSMETA_APP].Estimate = new SearchEstimateObject();
                keyStore[Constants.BELSMETA_APP].Estimate.DocName = Constants.BLSMT_ESTIMATE_DOC_NAME;
                keyStore[Constants.BELSMETA_APP].Estimate.BuildingName = Constants.BLSMT_ESTIMATE_BUILDING_NAME;
                keyStore[Constants.BELSMETA_APP].Estimate.BuildingCode = Constants.BLSMT_ESTIMATE_BUILDING_CODE;
                keyStore[Constants.BELSMETA_APP].Estimate.DrawingKit = Constants.BLSMT_ESTIMATE_DRAWING_KIT;
                keyStore[Constants.BELSMETA_APP].Estimate.StartLineLookingForEstimateName = Constants.BLSMT_ESTIMATE_START_LINE_LOOKING_FOR_ESTIMATE_NAME;

                keyStore[Constants.BELSMETA_APP].LaborCost = new SearchObject();
                keyStore[Constants.BELSMETA_APP].LaborCost.DocName = Constants.BLSMT_LABOR_COST_DOC_NAME;
                keyStore[Constants.BELSMETA_APP].LaborCost.ColName = Constants.BLSMT_LABOR_COST_COL_NAME;
                keyStore[Constants.BELSMETA_APP].LaborCost.RowName = Constants.BLSMT_LABOR_COST_ROW_NAME;

                keyStore[Constants.BELSMETA_APP].ContractCost = new SearchObject();
                keyStore[Constants.BELSMETA_APP].ContractCost.DocName = Constants.BLSMT_CONTRACT_COST_DOC_NAME;
                keyStore[Constants.BELSMETA_APP].ContractCost.ColName = Constants.BLSMT_CONTRACT_COST_COL_NAME;
                keyStore[Constants.BELSMETA_APP].ContractCost.RowName = Constants.BLSMT_CONTRACT_COST_ROW_NAME;

                keyStore[Constants.BELSMETA_APP].DoneSmrCost = new SearchObject();
                keyStore[Constants.BELSMETA_APP].DoneSmrCost.DocName = Constants.BLSMT_DONE_SMR_COST_DOC_NAME;
                keyStore[Constants.BELSMETA_APP].DoneSmrCost.ColName = Constants.BLSMT_DONE_SMR_COST_COL_NAME;
                keyStore[Constants.BELSMETA_APP].DoneSmrCost.RowName = Constants.BLSMT_DONE_SMR_COST_ROW_NAME;
                keyStore[Constants.BELSMETA_APP].DoneSmrCost.ExtraColName = Constants.BLSMT_DONE_SMR_COST_EXTRA_COL_NAME;
                keyStore[Constants.BELSMETA_APP].DoneSmrCost.ExtraRowName = Constants.BLSMT_DONE_SMR_COST_EXTRA_ROW_NAME;
            }
            #endregion

            Finding? result = new Finding();
            var s = keyStore.TryGetValue(type, out result);
            return result;
        }
    }
}