using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models.Settings;
using Microsoft.Extensions.Options;
using Quartz;

namespace MvcLayer.Scheduling
{
    public class CreateReportJob : IJob
    {
        private readonly IAdminService _adminService;
        private readonly ILoggerContract _loggerContract;
        private readonly SchedulerOptions _schedulerOptions;
        private readonly ExcelActivityReportOptions _reportOptions;
        private readonly IEmailService _emailService;
        private readonly EmailRecipient _recipient;
        private readonly IWebHostEnvironment _host;

        public CreateReportJob(
            IAdminService adminService,
            ILoggerContract loggerContract,
            IOptions<SchedulerOptions> schedulerOptions,
            IOptions<ExcelActivityReportOptions> reportOptions,
            IOptions<EmailRecipient> recipient,
            IEmailService emailService,
            IWebHostEnvironment host)
        {
            _adminService = adminService;
            _loggerContract = loggerContract;
            _schedulerOptions = schedulerOptions.Value;
            _reportOptions = reportOptions.Value;
            _recipient = recipient.Value;
            _emailService = emailService;
            _host = host;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var days = _schedulerOptions?.DaysCount > 0 ? _schedulerOptions.DaysCount : 7;
                _adminService.GetListActivity(days);

                var directoryPath = Path.Combine(_host.WebRootPath ?? string.Empty, (_reportOptions.Directory ?? string.Empty).TrimStart('\\', '/'));
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var filePath = Path.Combine(directoryPath, (_reportOptions.FileName ?? string.Empty) + (_reportOptions.FileType ?? string.Empty));
                var waited = 0;
                while (!File.Exists(filePath) && waited < 60)
                {
                    await Task.Delay(1000);
                    waited++;
                }

                if (!File.Exists(filePath))
                {
                    return;
                }

                byte[] bytes = File.ReadAllBytes(filePath);
                string period = $" c {DateTime.Now.AddDays(-days).ToShortDateString()} по {DateTime.Now.ToShortDateString()}";
                if (_recipient?.ReportActivities != null)
                {
                    string logoImgHtml = string.Empty;
                    try
                    {
                        var logoPath = Path.Combine(_host.WebRootPath ?? string.Empty, "Images", "ЛОГОТИП БЭС.jpg");
                        if (File.Exists(logoPath))
                        {
                            var logoBytes = File.ReadAllBytes(logoPath);
                            var base64 = Convert.ToBase64String(logoBytes);
                            var dataUrl = $"data:image/jpeg;base64,{base64}";
                            logoImgHtml = $"<img src='{dataUrl}' alt='БЭС' style='width:44px; height:44px; object-fit:contain; border-radius:6px; background:#ffffff22;' />";
                        }
                    }
                    catch { }
                    foreach (var item in _recipient.ReportActivities)
                    {
                        _emailService.SendAsync(item,
                            message:
                            $@"<div style='font-family:Segoe UI, Roboto, Arial, sans-serif; background:#f6f8fb; padding:24px;'>
  <div style='max-width:720px; margin:0 auto; background:#ffffff; border-radius:12px; box-shadow:0 6px 18px rgba(0,0,0,0.07); overflow:hidden;'>
    <div style='display:flex; align-items:center; justify-content:space-between; background:linear-gradient(135deg,#2457D6,#3BA3F8); padding:16px 20px; color:#fff;'>
      <div style='display:flex; align-items:center; gap:12px;'>
        {logoImgHtml}
        <div>
          <h2 style='margin:0; font-weight:600; font-size:18px;'>Отчет по активности пользователей</h2>
          <div style='opacity:.9; font-size:12px;'>Анализ и учет заключенных договоров</div>
        </div>
      </div>
      
    </div>
    <div style='padding:24px 24px 8px; color:#0f172a;'>
      <p style='margin:0 0 8px; font-size:14px; color:#334155;'>Период отчета:</p>
      <div style='display:inline-block; padding:8px 12px; background:#eef2ff; color:#1e293b; border-radius:8px; font-weight:600; font-size:14px;'>{period}</div>
      <hr style='border:none; border-top:1px solid #e5e7eb; margin:20px 0;' />
      <p style='margin:0; font-size:13px; color:#475569;'>Во вложении — файл с детализированной активностью пользователей за указанный период.</p>
    </div>
    <div style='padding:16px 24px 24px; color:#64748b; font-size:12px;'>
      <div style='display:flex; align-items:center; gap:8px; justify-content:space-between;'>
        <div style='display:flex; align-items:center; gap:8px;'>
          <div style='width:8px; height:8px; background:#22c55e; border-radius:50%;'></div>
          <span>Отдел СИСиА</span>
        </div>
        <div>Тел.: <a href='tel:2700616' style='color:#334155; text-decoration:none;'>270‑06‑16</a></div>
      </div>
    </div>
  </div>
</div>",
                            subject: "Отчет по активности пользователей",
                            attachment: new Attachment
                            {
                                FileName = _reportOptions.FileName + _reportOptions.FileType,
                                Bytes = bytes
                            });
                    }
                }

            }
            catch (Exception ex)
            {
                _loggerContract.WriteLog(LogLevel.Error, ex.Message, typeof(CreateReportJob).Name, nameof(Execute));
            }
        }
    }
}


