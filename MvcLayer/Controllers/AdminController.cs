using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces.PRO;
using BusinessLayer.Interfaces.Core;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.PRO;
using BusinessLayer.Models.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace MvcLayer.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class AdminController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAdminService _admin;
        private readonly IEmailService _emailService;
        private readonly EmailRecipient _recipient;
        private readonly IAbbreviationKindOfWorkService _abbreviationKind;
        private readonly ExcelActivityReportOptions _reportOptions;
        private readonly IHostingEnvironment _host;

        public AdminController(IMapper mapper, IAdminService adminService, IEmailService emailService, IOptions<EmailRecipient> options,
        IAbbreviationKindOfWorkService abbreviationKind, IOptions<ExcelActivityReportOptions> excelActiv,
            IHostingEnvironment hosting)
        {
            _mapper = mapper;
            _admin = adminService;
            _emailService = emailService;
            _recipient = options.Value;
            _abbreviationKind = abbreviationKind;
            _reportOptions = excelActiv.Value;
            _host = hosting;
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult CreateReport(int daysCount)
        {
            try
            {
                _admin.GetListActivity(daysCount);
            }
            catch (Exception)
            {
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Policy = "CreatePolicy")]
        public ActionResult SendReport()
        {
            try
            {
                if (!Directory.Exists(_host.WebRootPath + _reportOptions.Directory))
                {
                    Directory.CreateDirectory(_host.WebRootPath + _reportOptions.Directory);
                }
                string fileName = _host.WebRootPath + _reportOptions.Directory + _reportOptions.FileName + _reportOptions.FileType;
                byte[] bytes = System.IO.File.ReadAllBytes(fileName);
                string period = $" c {DateTime.Now.AddDays(-7).ToShortDateString()} по {DateTime.Now.ToShortDateString()}";

                string logoImgHtml = string.Empty;

                var logoPath = Path.Combine(_host.WebRootPath ?? string.Empty, "Images", "ЛОГОТИП БЭС.jpg");
                if (System.IO.File.Exists(logoPath))
                {
                    var logoBytes = System.IO.File.ReadAllBytes(logoPath);
                    var base64 = Convert.ToBase64String(logoBytes);
                    var dataUrl = $"data:image/jpeg;base64,{base64}";
                    logoImgHtml = $"<img src='{dataUrl}' alt='БЭС' style='width:44px; height:44px; object-fit:contain; border-radius:6px; background:#ffffff22;' />";
                }


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
            catch (Exception)
            {
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
