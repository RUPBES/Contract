using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class PaymentService: IPaymentService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;

        public PaymentService(IContractUoW database, IMapper mapper, ILoggerContract logger)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;           
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

        public IEnumerable<PaymentDTO> Find(Func<Payment, bool> predicate)
        {
            return _mapper.Map<IEnumerable<PaymentDTO>>(_database.Payments.Find(predicate));
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
    }
}