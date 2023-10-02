using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BusinessLayer.Services
{
    internal class PaymentService: IPaymentService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;

        public PaymentService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
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
                    _logger.WriteLog(LogLevel.Information, $"create payment, ID={payment.Id}", typeof(PaymentService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    return payment.Id;
                }
            }

            _logger.WriteLog(LogLevel.Warning, $"not create payment, object is null", typeof(PaymentService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

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
                        _logger.WriteLog(LogLevel.Information, $"delete payment, ID={id}", typeof(PaymentService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(PaymentService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete payment, ID is not more than zero", typeof(PaymentService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
                _logger.WriteLog(LogLevel.Information, $"update payment, ID={item.Id}", typeof(PaymentService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update payment, object is null", typeof(PaymentService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }
    }
}