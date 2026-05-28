using BusinessLayer.Models.KDO;
using BusinessLayer.Models.Settings;
using DatabaseLayer.Models.OID;

namespace BusinessLayer.Interfaces.Core
{
    public interface IAdminService
    {
        void GetListActivity(int days);
        Task<IEnumerable<UserDashboardDTO>> GetFullUserInfo();
        Task<IEnumerable<UserActivityDto>> GetDashboardUserInfo();
        Task<Dictionary<string, List<ActivityTimelinePoint>>> GetActivityTimelinePoints();

        Task<IEnumerable<LogDTO>> GetLogs();
    }
}