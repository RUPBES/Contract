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

        public ActiveUsersService(IConverterService converter, IHttpContextUserProvider httpHelper, IContractUoW contract,
            IMapper mapper, IExcelWriter excelWriter, IContractsLogger loggerContract, IOptions<ExcelActivityReportOptions> options
            , IHostingEnvironment hosting)
        {
            _converter = converter;
            _httpHelper = httpHelper;
            _contract = contract;
            _mapper = mapper;
            _excelWriter = excelWriter;
            _loggerContract = loggerContract;
            _reportOptions = options.Value;
            _host = hosting;
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
                var sheet = _excelWriter.Settup(path, _reportOptions.SheetName);

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
                    _excelWriter.WriteLine(sheet, startRow++, path, true, null, width: null,isTextWrap:null, ExcelHorizontalAlignment.Left, rowItems.ToArray());

                    startCol = 1;
                    foreach (var user in users)
                    {
                        var organization = _httpHelper.GetUserOrganization(user.Key);

                        foreach (var item in user)
                        {
                            _excelWriter.WriteLine(sheet, startRow, path, false, null, null, isTextWrap: null, align: null,
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
                        _excelWriter.WriteLine(sheet, startRow++, path, false, null, null, isTextWrap: null, align: null, new RowItem { Value = string.Empty, Col = startCol });
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
