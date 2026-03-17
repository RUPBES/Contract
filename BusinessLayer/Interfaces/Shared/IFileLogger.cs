namespace BusinessLayer.Interfaces.Shared
{
    public interface IFileLogger:IContractsLogger
    {
        void LogFile(string message);
    }
}
