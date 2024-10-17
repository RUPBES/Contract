using Microsoft.Extensions.Logging;

namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface ILoggerContract
    {
        void WriteLog(LogLevel logLevel, string message, string nameSpace = null, string methodName = null);
    }
}
