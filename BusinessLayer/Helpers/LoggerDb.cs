using BusinessLayer.Interfaces.CommonInterfaces;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Helpers
{
    internal class LoggerDb : ILoggerContract
    {
        private readonly IContractUoW _contract;
        private readonly IHttpHelper _httpHelper;
        public LoggerDb(IContractUoW contract, IHttpHelper httpHelper)
        {
            _contract = contract;
            _httpHelper = httpHelper;
        }

        public void WriteLog(LogLevel logLevel, string message, string nameSpace = null, string methodName = null)
        {
            try
            {
                _contract.Logs.Create(new Log
                {
                    LogLevel = logLevel.ToString(),
                    Message = message,
                    NameSpace = nameSpace,
                    MethodName = methodName,
                    UserName = _httpHelper.GetUserName(),
                    DateTime = DateTime.Now,
                    UserIdentifierOid = _httpHelper.GetUserIdentifierOid()
                });
                _contract.Save();               
            }
            catch (Exception)
            {
            }
        }
    }
}