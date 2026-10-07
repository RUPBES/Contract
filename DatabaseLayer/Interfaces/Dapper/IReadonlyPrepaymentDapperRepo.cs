using DatabaseLayer.Models.EXTRA;
using DatabaseLayer.Models.KDO;


namespace DatabaseLayer.Interfaces.Dapper
{
    public interface IReadonlyPrepaymentDapperRepo : IReadonlyRepoDapper<Prepayment>
    {
        Task<List<PeriodAdvance>> GetPeriodAdvancesAsync(
            int contractId,
            DateTime startPeriod,
            DateTime endPeriod,
            string? databaseName = null);

        Task<PrepaymentSchedule?> GetTotalsAsync(int contractId, string? databaseName = null);
    }
}