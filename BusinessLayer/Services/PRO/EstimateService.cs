using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using BusinessLayer.Models.PRO;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.Models.PRO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class EstimateService : IEstimateService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public EstimateService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
        }

        public int? Create(EstimateDTO item)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

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
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

                    return estimate.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create Estimate, object is null",
                            nameSpace: typeof(EstimateService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

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
                            message: $"delete employee, ID={id}",
                            nameSpace: typeof(EstimateService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(EstimateService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete Estimate, ID is not more than zero",
                            nameSpace: typeof(EstimateService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
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
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            if (item is not null)
            {
                _database.Estimates.Update(_mapper.Map<Estimate>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update Estimate, ID={item.Id}",
                            nameSpace: typeof(EstimateService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update Estimate, object is null",
                            nameSpace: typeof(EstimateService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name,
                            userName: user);
            }
        }

        public IndexViewModel GetPage(int pageSize, int pageNum, string org)
        {            
            int skipEntities = (pageNum - 1) * pageSize;
            var items = _database.Estimates.GetEntityWithSkipTake(skipEntities,pageSize, org).OrderBy(x => x.BuildingCode);
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
            //TODO: erfewr добавить и убувить!!
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
    }
}
