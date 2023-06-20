using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Helpers
{
    internal class LoggerDb : ILoggerContract
    {
        private readonly IContractUoW _contract;
        private readonly IMapper _mapper;
        public LoggerDb(IContractUoW contract, IMapper mapper)
        {
            _contract = contract;
            _mapper = mapper;
        }

        public void WriteLog(LogLevel logLevel, string message, string nameSpace = null, string methodName = null, string userName = null)
        {
            try
            {
                LogDTO log = new LogDTO();

                log.LogLevel = logLevel.ToString();
                log.Message = message;
                log.NameSpace = nameSpace;
                log.MethodName = methodName;
                log.UserName = userName;
                log.Date = DateTime.Now;

                _contract.Logs.Create(_mapper.Map<Log>(log));
            }
            catch (Exception)
            {
            }
        }
    }
}