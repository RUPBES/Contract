using AutoMapper;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces.PRO;
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
                foreach (var item in _recipient.ReportActivities)
                {
                    _emailService.SendAsync(item,
                        message:
                        $"<div style='display:flex;flex-direction:column; justify-content:space-between;'>" +
                            $"<div style='display:flex;justify-content:center; text-align:centr;'>" +
                                $"<b>Еженедельная рассылка отчета по активности пользователей в программе " +
                                $"<br/> <span style='color:red'> \"Анализ и учет заключенных договоров\"</span></b>" +

                        $"</div>" +
                        $"<div style='display:flex;justify-content:center; text-align:centr;'>Период отчета: {period}</div><hr/>" +
                        $"<div style='display:flex;text-align:center;'>Отдел СИСиА</div>" +
                        $"</div>",
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
