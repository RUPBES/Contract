using AutoMapper;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.COMServices;
using BusinessLayer.Interfaces.Core;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using BusinessLayer.Models.Settings;
using BusinessLayer.ServicesCOM;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.Models.OID;
using DatabaseLayer.UOW;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml.Style;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Reflection;

namespace BusinessLayer.Services.Administrator
{
    public class ActiveUsersService : IAdminService
    {
        private readonly IConverterService _converter;
        private readonly IHttpContextUserProvider _httpHelper;
        private readonly IContractUoW _contract;
        private readonly IMapper _mapper;
        private readonly IExcelWriter _excelWriter;
        private readonly ExcelActivityReportOptions _reportOptions;
        private readonly IHostingEnvironment _host;
        private readonly IContractsLogger _loggerContract;
        private readonly IOpenIdDictUoW _openIdDictCntxt;

        public ActiveUsersService(IConverterService converter, IHttpContextUserProvider httpHelper, IContractUoW contract,
            IMapper mapper, IExcelWriter excelWriter, IContractsLogger loggerContract, IOptions<ExcelActivityReportOptions> options
            , IHostingEnvironment hosting, IOpenIdDictUoW openIdDictUoW)
        {
            _converter = converter;
            _httpHelper = httpHelper;
            _contract = contract;
            _mapper = mapper;
            _excelWriter = excelWriter;
            _loggerContract = loggerContract;
            _reportOptions = options.Value;
            _host = hosting;
            _openIdDictCntxt = openIdDictUoW;
        }

        public void GetListActivity(int daysCount)
        {
            try
            {
                var dateStart = DateTime.Now.Date.AddDays((-1) * daysCount);
                var logs = _contract.Logs.Find(x => x.DateTime?.Date > dateStart.Date).ToList();
                WriteExcelReportAsync(logs);
            }
            catch (Exception e)
            {
                _loggerContract.WriteLog(LogLevel.Error, e.Message, typeof(ActiveUsersService).Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Метод возвращает список сотрудников со всей информаций для дашборда. ЕСЛИ кого-то нет, то у пользователя отсутствует в БД scope=ContractApplicationMVC
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<UserDashboardDTO>> GetFullUserInfo()
        {
            var logs = await _contract.Logs.GetAllAsync();
            var sdsdsd = await _openIdDictCntxt.AbpUsers.GetUsersInfoAsync();

            var lastActivityByUser = logs?
                  .Where(l => !string.IsNullOrEmpty(l.UserIdentifierOid) && l.DateTime.HasValue)
                  .GroupBy(l => l.UserIdentifierOid!.ToUpperInvariant())
                  .Select(g => new
                  {
                      UserId = g.Key,
                      LastActivity = g.Max(l => l.DateTime!.Value)
                  })
                  .ToDictionary(
                      k => k.UserId,
                      v => v.LastActivity
                  );


            return sdsdsd.Select(x => new UserDashboardDTO
            {
                Id = x.Id,
                UserUniqName = x.UserUniqName,
                Name = x.Name,
                Surname = x.Surname,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                ExtraProperties = x.ExtraProperties,
                CreationTime = x.CreationTime,
                CreatorId = x.CreatorId,
                IsDeleted = x.IsDeleted,
                FullName = (x.Surname + " " + x.Name),

                OrgId = x.OrgId,
                OrgCode = x.OrgCode,
                OrgDisplayName = x.OrgDisplayName,
                ParentOrgId = x.ParentOrgId,

                DepartCode = x.DepartCode,
                DepartDisplayName = x.DepartDisplayName,
                DepartId = x.DepartId,

                JobId = x.JobId,
                JobCode = x.JobCode,
                JobDisplayName = x.JobDisplayName,

                LastActivity = lastActivityByUser != null &&
                   lastActivityByUser.TryGetValue(x.Id.ToString().ToUpper(), out var activity)
                   ? activity : null
            })
                .ToList();


            //return  _mapper.Map<IEnumerable<UserDashboardDTO>>(await _openIdDictCntxt.AbpUsers.GetUsersInfoAsync());
        }

        public async Task<IEnumerable<UserActivityDto>> GetDashboardUserInfo()
        {
            var logs = await _contract.Logs.GetAllAsync();
            var userInfoDashboard = await _openIdDictCntxt.AbpUsers.GetUsersInfoAsync();
            return logs.GroupBy(x => x.UserName).Select(g => new UserActivityDto
            {
                NameIdentifier = g?.FirstOrDefault()?.UserIdentifierOid,
                Email = userInfoDashboard?.FirstOrDefault(x => x.FullName == g?.FirstOrDefault()?.UserName)?.Email ?? string.Empty,
                FullName = g.Key,
                CreationTime = (DateTime)g?.FirstOrDefault()?.DateTime,
                LastActivity = g?.OrderByDescending(x => x.DateTime)?.FirstOrDefault()?.DateTime,
                Creates = g.Count(s => s.MethodName == "Create" || s.MethodName == "AttachFileToEntity" || s.MethodName == "AddFile"
                || s.MethodName == "AddAmendmentToPrepayment" || s.MethodName == "AddAmendmentToMaterial" || s.MethodName == "AddAmendmentToScopeWork"),
                Updates = g.Count(s => s.MethodName == "Update"),
                Deletes = g.Count(s => s.MethodName == "Delete"),

            }).ToList();
        }

        public async Task<Dictionary<string, List<ActivityTimelinePoint>>> GetActivityTimelinePoints()
        {
            var now = DateTime.Now;
            var oneYearAgo = now.AddYears(-1);

            var logsYear = await _contract.Logs.FindAsync(x => x.DateTime >= oneYearAgo);

            // Фильтруем один раз, не повторяя обращения к DateTime.Now
            var logsMonth = logsYear.Where(x => x.DateTime?.Date >= now.AddMonths(-1).Date).ToList();
            var logsDay = logsYear.Where(x => x.DateTime?.Date == now.Date).ToList();

            string[] monthNames = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн",
                        "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };

            var ds = new Dictionary<string, List<ActivityTimelinePoint>>
            {
                ["year"] = BuildYearTimeline(logsYear, monthNames),
                ["month"] = BuildMonthTimeline(logsMonth, now),
                ["day"] = BuildDayTimeline(logsDay),
            };
            return ds;
        }

        public async Task<IEnumerable<LogDTO>> GetLogs()
        {
            var logs = await _contract.Logs.GetAllAsync();
            return _mapper.Map<IEnumerable<LogDTO>>(logs);
        }



        private static List<ActivityTimelinePoint> BuildYearTimeline(
            IEnumerable<Log> logs, string[] monthNames)
        {
            var dated = logs.Where(x => x.DateTime.HasValue).ToList();
            if (dated.Count == 0)
                return new List<ActivityTimelinePoint>();

            var minDate = new DateTime(dated.Min(x => x.DateTime!.Value.Ticks));
            var maxDate = new DateTime(dated.Max(x => x.DateTime!.Value.Ticks));

            var countByMonth = dated
                .GroupBy(x => new DateTime(x.DateTime!.Value.Year, x.DateTime.Value.Month, 1))
                .ToDictionary(g => g.Key, g => g.Count());

            return MonthRange(minDate, maxDate)
                .Select(d => new ActivityTimelinePoint
                {
                    Label = $"{monthNames[d.Month - 1]} {d.Year}",
                    Value = countByMonth.GetValueOrDefault(d, 0)
                })
                .ToList();
        }

        private static List<ActivityTimelinePoint> BuildMonthTimeline(
            IEnumerable<Log> logs, DateTime now)
        {
            var countByDay = logs
                .Where(x => x.DateTime.HasValue && x.DateTime.Value.Month == now.Month)
                .GroupBy(x => x.DateTime!.Value.Day)
                .ToDictionary(g => g.Key, g => g.Count());

            return Enumerable.Range(1, DateTime.DaysInMonth(now.Year, now.Month))
                .Select(day => new ActivityTimelinePoint
                {
                    Label = $"{day:D2}.{now.Month:D2}",
                    Value = countByDay.GetValueOrDefault(day, 0)
                })
                .ToList();
        }

        private static List<ActivityTimelinePoint> BuildDayTimeline(IEnumerable<Log> logs)
        {
            var dated = logs.Where(x => x.DateTime.HasValue).ToList();

            // Если логов нет — показываем полные сутки
            int minHour = /*dated.Count > 0 ? dated.Min(x => x.DateTime!.Value.Hour) :*/ 7;
            int maxHour =/* dated.Count > 0 ? dated.Max(x => x.DateTime!.Value.Hour) :*/ 21;

            var countByHour = dated
                .GroupBy(x => x.DateTime!.Value.Hour)
                .ToDictionary(g => g.Key, g => g.Count());

            return Enumerable.Range(minHour, maxHour - minHour + 1)
                .Select(hour => new ActivityTimelinePoint
                {
                    Label = $"{hour:D2}:00",
                    Value = countByHour.GetValueOrDefault(hour, 0)
                })
                .ToList();
        }

        // Генератор диапазона месяцев
        private static IEnumerable<DateTime> MonthRange(DateTime from, DateTime to)
        {
            var current = new DateTime(from.Year, from.Month, 1);
            var end = new DateTime(to.Year, to.Month, 1);
            while (current <= end)
            {
                yield return current;
                current = current.AddMonths(1);
            }
        }

        private async void WriteExcelReportAsync(IEnumerable<Log> logs)
        {
            await Task.Run(() =>
            {
                var users = ConvertVariableToRussianName(logs);
                if (users is null)
                {
                    _loggerContract.WriteLog(LogLevel.Warning, "This period don't have activities of users", typeof(ActiveUsersService).Name, MethodBase.GetCurrentMethod()?.Name);
                    return;
                }

                if (!Directory.Exists(_host.WebRootPath + _reportOptions.Directory))
                {
                    Directory.CreateDirectory(_host.WebRootPath + _reportOptions.Directory);
                }

                string path = _host.WebRootPath + _reportOptions.Directory + _reportOptions.FileName + _reportOptions.FileType;
                var sheet = _excelWriter.Settup(path, nameSheets: _reportOptions.SheetName);

                int startRow = 2;
                int startCol = 1;
                var colorText = Color.Black;
                var colorTextHdr = Color.White;
                var colorBcgHdr = Color.DarkBlue;
                List<string> listHeaders = new List<string> { "Сотрудник", "Организация", "Должность", "Сервис", "Действие", "Дата доступа" };
                try
                {
                    List<RowItem> rowItems = new List<RowItem>();
                    foreach (var colName in listHeaders)
                    {
                        rowItems.Add(new RowItem
                        {
                            Value = colName,
                            Col = startCol++,
                            FontColor = Constants.COLOR_WHITE,
                            FontSize = Constants.FONT_SIZE_18,
                            BgColor = Constants.COLOR_DARK_BLUE
                        });
                    }
                    _excelWriter.WriteLine(sheet, startRow++, path, true, null, width: null, isTextWrap: null, ExcelHorizontalAlignment.Left, values: rowItems.ToArray());

                    startCol = 1;
                    foreach (var user in users)
                    {
                        var organization = _httpHelper.GetUserOrganization(user.Key);

                        foreach (var item in user)
                        {
                            _excelWriter.WriteLine(sheet, startRow, path, false, null, null, isTextWrap: null, align: null, null, null,
                                new RowItem { Value = item.UserName, Col = startCol++, FontColor = colorText, FontSize = Constants.FONT_SIZE_14 },
                                new RowItem { Value = organization?.enterprise, Col = startCol++, FontColor = colorText, FontSize = Constants.FONT_SIZE_14 },
                                new RowItem { Value = organization?.position, Col = startCol++, FontColor = colorText, FontSize = Constants.FONT_SIZE_14 },
                                new RowItem { Value = item.NameSpace, Col = startCol++, FontColor = colorText, FontSize = Constants.FONT_SIZE_14 },
                                new RowItem { Value = item.MethodName, Col = startCol++, FontColor = colorText, FontSize = Constants.FONT_SIZE_14 },
                                new RowItem { Value = item.DateTime, Col = startCol++, FontColor = colorText, FontSize = Constants.FONT_SIZE_14 }
                                );
                            startCol = 1;
                            startRow++;
                        }
                        _excelWriter.WriteLine(sheet, startRow++, path, false, null, null, isTextWrap: null, align: null, values: new RowItem { Value = string.Empty, Col = startCol });
                    }
                    _excelWriter.CloseExcel();
                }
                catch (Exception)
                {
                    _excelWriter.CloseExcel();
                }
            });
        }

        private IEnumerable<IGrouping<string?, UserActivity>>? ConvertVariableToRussianName(IEnumerable<Log> logs)
        {
            var newList = new List<UserActivity>();

            if (logs?.Count() > 0)
            {
                foreach (var log in logs)
                {
                    newList.Add(new UserActivity
                    {
                        DateTime = log.DateTime?.ToShortDateString(),
                        UserName = log?.UserName,
                        MethodName = _converter?.ToRussianMethodName(log.MethodName),
                        NameSpace = _converter?.ToRussianNameSpace(log.NameSpace)
                    });
                }
                return newList.GroupBy(x => x.UserName);
            }
            return null;
        }

    }
}
