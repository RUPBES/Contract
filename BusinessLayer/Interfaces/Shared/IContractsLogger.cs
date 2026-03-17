using Microsoft.Extensions.Logging;

namespace BusinessLayer.Interfaces.Shared
{
    public interface IContractsLogger
    {
        void WriteLog(LogLevel logLevel, string message, string nameSpace = null, string methodName = null);
    }
}
