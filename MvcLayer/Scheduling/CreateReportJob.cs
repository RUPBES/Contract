using BusinessLayer.Interfaces.Core;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.Settings;
using Microsoft.Extensions.Options;
using Quartz;

namespace MvcLayer.Scheduling
{
    public class CreateReportJob : IJob
    {
        private readonly IAdminService _adminService;
        private readonly IContractsLogger _loggerContract;
        private readonly SchedulerOptions _schedulerOptions;

        public CreateReportJob(
            IAdminService adminService,
            IContractsLogger loggerContract,
            IOptions<SchedulerOptions> schedulerOptions)
        {
            _adminService = adminService;
            _loggerContract = loggerContract;
            _schedulerOptions = schedulerOptions.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var days = _schedulerOptions?.DaysCount > 0 ? _schedulerOptions.DaysCount : 7;
                _adminService.GetListActivity(days);               
            }
            catch (Exception ex)
            {
                _loggerContract.WriteLog(LogLevel.Error, ex.Message, typeof(CreateReportJob).Name, nameof(Execute));
            }
        }
    }
}


