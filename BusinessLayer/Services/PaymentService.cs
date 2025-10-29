using AutoMapper;
using Azure;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using BusinessLayer.Models.Settings;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Interfaces.Entities;
using DatabaseLayer.Models.KDO;
using Microsoft.Extensions.Logging;
using System.Drawing.Printing;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class PaymentService : IPaymentService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly IContractArchiveUoW _databaseArch;
        private readonly ILoggerContract _logger;
        private readonly IReadonlyPaymentDapperRepo _paymentCash;

        public PaymentService(IContractUoW database, IMapper mapper, ILoggerContract logger, IContractArchiveUoW databaseArch, IReadonlyPaymentDapperRepo paymentCash)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _databaseArch = databaseArch;
            _paymentCash = paymentCash;
        }

        public int? Create(PaymentDTO item)
        {
            if (item is not null)
            {
                if (_database.Payments.GetById(item.Id) is null)
                {
                    var payment = _mapper.Map<Payment>(item);

                    _database.Payments.Create(payment);
                    _database.Save();

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create payment, ID={payment.Id}",
                            nameSpace: typeof(PaymentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    return payment.Id;
                }
            }

            _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create payment, object is null",
                            nameSpace: typeof(PaymentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

            return null;
        }

        public void Delete(int id, int? secondId = null)
        {
            if (id > 0)
            {
                var payment = _database.Payments.GetById(id);

                if (payment is not null)
                {
                    try
                    {
                        _database.Payments.Delete(id);
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"delete payment, ID={id}",
                            nameSpace: typeof(PaymentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                            logLevel: LogLevel.Error,
                            message: e.Message,
                            nameSpace: typeof(PaymentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not delete payment, ID is not more than zero",
                            nameSpace: typeof(PaymentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<PaymentDTO> Find(Func<Payment, bool> predicate, bool? useArchiveData)
        {
            return (useArchiveData == true) ?
                _mapper.Map<IEnumerable<PaymentDTO>>(_databaseArch.Payments.Find(predicate)) :
                 _mapper.Map<IEnumerable<PaymentDTO>>(_database.Payments.Find(predicate));
        }

        public IEnumerable<PaymentDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<PaymentDTO>>(_database.Payments.GetAll());
        }

        public PaymentDTO GetById(int id, int? secondId = null)
        {
            var payment = _database.Payments.GetById(id);

            if (payment is not null)
            {
                return _mapper.Map<PaymentDTO>(payment);
            }
            else
            {
                return null;
            }
        }

        public void Update(PaymentDTO item)
        {
            if (item is not null)
            {
                _database.Payments.Update(_mapper.Map<Payment>(item));
                _database.Save();

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update payment, ID={item.Id}",
                            nameSpace: typeof(PaymentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update payment, object is null",
                            nameSpace: typeof(PaymentService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        //()
        public IndexViewModel GetPayableCash(int pageSize, int page, FilterPayableModel filter, string[] organizationName, bool? useArchiveData)
        {
            int skip = (page - 1) * pageSize;
            int count = _paymentCash.Count();
            string queryString = CreateQueryString(filter);

            var items = _mapper.Map<IEnumerable<VPaymentCashDTO>>(_paymentCash.GetEntitySkipTake(skip, pageSize, queryString, organizationName));

            PageViewModel pageViewModel = new PageViewModel(count, page, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Objects = items
            };

            return viewModel;
        }

        public IEnumerable<VPaymentCashDTO> GetPayableCash(FilterPayableModel filter, string[] organizationName)
        { 
            string queryString = CreateQueryString(filter);
            return _mapper.Map<IEnumerable<VPaymentCashDTO>>(_paymentCash.Find(queryString, organizationName));                    
        }

        private string CreateQueryString(FilterPayableModel filter)
        {
            string queryString = string.Empty;

            if (filter?.Client != null && filter?.Client?.Equals("null", StringComparison.OrdinalIgnoreCase) == false)
            {
                queryString += $"and Client ='{filter.Client}'";
            }
            if (filter?.GenContractor != null && filter?.GenContractor?.Equals("null", StringComparison.OrdinalIgnoreCase) == false)
            {
                queryString += $"and GenContractor ='{filter.GenContractor}'";
            }

            if (filter?.DateEnteringTerm != null && filter.DateEnteringTerm != default)
            {
                queryString += $"and EnteringTerm ='{filter.DateEnteringTerm}'";
            }
            else if (filter?.StarEnteringTerm != null && filter?.EndEnteringTerm != null)
            {
                queryString += $"and EnteringTerm >='{filter.StarEnteringTerm}' and EnteringTerm <='{filter.EndEnteringTerm}'";
            }
            else if (filter?.StarEnteringTerm != null && filter.EndEnteringTerm == null)
            {
                queryString += $"and EnteringTerm >='{filter.StarEnteringTerm}'";
            }
            else if (filter?.StarEnteringTerm == null && filter?.EndEnteringTerm != null)
            {
                queryString += $"and EnteringTerm <='{filter.EndEnteringTerm}'";
            }

            return queryString;
        }
    }
}