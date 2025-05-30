using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.CommonInterfaces;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml.Style;
using System.Reflection;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using OfficeOpenXml;
using BusinessLayer.Models;

namespace BusinessLayer.ServicesCOM
{
    internal class ReportExcelService : IReportExcelService
    {
        private readonly IConverter _converter;
        private readonly IContractUoW _contract;
        private readonly IExcelWriter _excelWriter;
        private readonly IHostingEnvironment _host;
        private readonly ILoggerContract _loggerContract;
        private readonly IScopeWorkService _scopeWork;
        private readonly IFormService _formService;
        public ReportExcelService(IConverter converter, IContractUoW contract, IExcelWriter excelWriter,
            ILoggerContract loggerContract, IHostingEnvironment hosting, IScopeWorkService scopeWork, IFormService formService)
        {
            _converter = converter;
            _contract = contract;
            _excelWriter = excelWriter;
            _loggerContract = loggerContract;
            _host = hosting;
            _scopeWork = scopeWork;
            _formService = formService;
        }


        public async Task<string> ExportContracts(string organization, List<string> columnName)
        {
            if (!string.IsNullOrEmpty(organization) && organization != "all")
            {
                var contracts = _contract.vContracts.Find(x => x.Owner == organization).Select(c => GetProperties(c, columnName));
                return await ExportContractsExcelAsync(contracts);
            }
            else
            {
                var contracts = _contract.vContracts.GetAll().Select(c => GetProperties(c, columnName));
                return await ExportContractsExcelAsync(contracts);
            }
        }

        private async Task<string> ExportContractsExcelAsync(IEnumerable<Dictionary<string, string>> contracts)
        {
            if (!Directory.Exists(_host.WebRootPath + "\\Temp"))
            {
                Directory.CreateDirectory(_host.WebRootPath + "\\Temp");
            }

            string path = _host.WebRootPath + "\\Temp" + "\\Договора.xlsx";
            await Task.Run(() =>
            {
                var sheet = _excelWriter.Settup(path, "Учет договоров");
                int startRow = 2;
                int startCol = 1;

                try
                {
                    ///ЗАПОЛНЯЕМ ШАПКУ ТАБЛИЦЫ
                    List<RowItem> rowItems = new List<RowItem>();
                    foreach (var colName in contracts.FirstOrDefault().Select(x => x.Key))
                    {
                        rowItems.Add(new RowItem
                        {
                            Value = _converter.ToRussianContractProps(colName),
                            Col = startCol++,
                            FontColor = Constants.COLOR_WHITE,
                            FontSize = Constants.FONT_SIZE_14,
                            BgColor = Constants.COLOR_DARK_BLUE
                        });
                    }
                    _excelWriter.WriteLine(sheet, startRow++, path, true, width: null, isTextWrap: null, ExcelHorizontalAlignment.Left, rowItems.ToArray());

                    ///ЗАПОЛНЯЕМ ТАБЛИЦУ
                    startCol = 1;
                    foreach (var contractValues in contracts.Select(x => x.Values))
                    {
                        List<RowItem> rowItems2 = new List<RowItem>();
                        foreach (var value in contractValues)
                        {

                            rowItems2.Add(new RowItem
                            {
                                Value = value,
                                Col = startCol++,
                                FontColor = Constants.COLOR_BLACK,
                                FontSize = Constants.FONT_SIZE_14,
                            });
                        }
                        _excelWriter.WriteLine(sheet, startRow, path, false, width: 25, isTextWrap: true, align: null, rowItems2.ToArray());
                        startCol = 1;
                        startRow++;

                    }

                    _excelWriter.CloseExcel();
                }
                catch (Exception)
                {
                    _excelWriter.CloseExcel();
                }
            });
            return path;
        }

        public async Task<string> ExportScopeWorkToExcel(string fileName, int contractId, string? sheetName)
        {

            Directory.CreateDirectory(_host.WebRootPath + "\\Temp");
            string path = _host.WebRootPath + "\\Temp\\" + fileName;
            

            await Task.Run(() =>
            {
                var sheet = _excelWriter.Settup(path, sheetName);
                var contractProp = _contract.Contracts
                 .Find(x => x.Id == contractId)
                 .Select(x => new { x.IsAgreementContract, x.IsSubContract, x.IsOneOfMultiple, x.IsEngineering, x.DateBeginWork, x.DateEndWork, x.ContractTerm })
                 .FirstOrDefault();

                if (contractProp == null)
                {
                    return null;
                }

                var type = ScopeType.Both;

                if (contractProp.IsAgreementContract == true || contractProp.IsSubContract == true || contractProp.IsEngineering == true)
                {
                    type = ScopeType.NoOwn;
                }

                var scopes = _scopeWork.GetScopeWorksInfoTable(contractId, type);
                var forms = _formService.GetScopeWorksInfoTable(contractId, type);

                var dates = new List<DateTime>();
                var currentDate = contractProp?.DateBeginWork;

                while (currentDate <= contractProp?.DateEndWork)
                {
                    dates.Add((DateTime)currentDate);
                    currentDate = currentDate?.AddMonths(1);
                }


                int startRow = 2;
                int startCol = 1;

                try
                {
                    // Set headers
                    List<RowItem> headerItems = new List<RowItem>
                {
                    new RowItem { Value = "Вид работ", Col = startCol++, FontColor = Constants.COLOR_WHITE, FontSize = Constants.FONT_SIZE_14, BgColor = Constants.COLOR_DARK_BLUE },
                    new RowItem { Value = "Стоимость работ", Col = startCol++, FontColor = Constants.COLOR_WHITE, FontSize = Constants.FONT_SIZE_14, BgColor = Constants.COLOR_DARK_BLUE },
                    new RowItem { Value = "Выполнено на 01.01 тек.года", Col = startCol++, FontColor = Constants.COLOR_WHITE, FontSize = Constants.FONT_SIZE_14, BgColor = Constants.COLOR_DARK_BLUE },
                    new RowItem { Value = "Остаток по выполнению", Col = startCol++, FontColor = Constants.COLOR_WHITE, FontSize = Constants.FONT_SIZE_14, BgColor = Constants.COLOR_DARK_BLUE },
                    new RowItem { Value = "Объем на текущий год", Col = startCol++, FontColor = Constants.COLOR_WHITE, FontSize = Constants.FONT_SIZE_14, BgColor = Constants.COLOR_DARK_BLUE },
                };
                    List<RowItem> periodsRow = new List<RowItem>();

                    foreach (var date in dates)
                    {
                        periodsRow.Add(new RowItem
                        {
                            Value = date.ToString("MMM yyyy"),
                            Col = startCol++,
                            FontColor = Constants.COLOR_WHITE,
                            FontSize = Constants.FONT_SIZE_14,
                            BgColor = Constants.COLOR_DARK_BLUE
                        });
                    }

                    headerItems.AddRange(periodsRow);
                    _excelWriter.WriteLine(sheet, startRow++, path, true, width: null, isTextWrap: null, ExcelHorizontalAlignment.Left, headerItems.ToArray());

                    // Set body
                    int nextRow = FillTable(sheet, scopes, startRow, path, dates);
                    FillTable(sheet, forms, nextRow, path, dates);

                    // Close Excel
                    _excelWriter.CloseExcel();

                }
                catch (Exception)
                {
                    _excelWriter.CloseExcel();
                    return string.Empty;
                }
                return path;
            });
            return path;
        }

        private Dictionary<string, string> GetProperties(VContract contract, List<string> props)
        {
            var values = new Dictionary<string, string>();

            foreach (var prop in props)
            {
                PropertyInfo propertyInfo = contract.GetType().GetProperty(prop);
                if (propertyInfo != null)
                {

                    if (propertyInfo.GetValue(contract) is DateTime)  //если дата, то приводим ее в читабельный короткий формат
                    {
                        try
                        {
                            DateTime date = (DateTime)propertyInfo.GetValue(contract);
                            values.Add(propertyInfo.Name, date.ToShortDateString() ?? string.Empty);
                        }
                        catch (Exception)
                        {
                            values.Add(propertyInfo.Name, string.Empty);
                        }

                    }
                    else if (propertyInfo.GetValue(contract) is Decimal)  //если стоимость, делаем округление до 2 цифр после запятой
                    {
                        try
                        {
                            Decimal money = (decimal)propertyInfo.GetValue(contract);
                            values.Add(propertyInfo.Name, money.ToString("N2") ?? string.Empty);
                        }
                        catch (Exception)
                        {
                            values.Add(propertyInfo.Name, string.Empty);
                        }
                    }
                    else
                    {
                        values.Add(propertyInfo.Name, propertyInfo.GetValue(contract)?.ToString() ?? string.Empty);
                    }
                }
            }

            return values;
        }

        private int FillTable(ExcelWorksheet sheet, ScopeWorkReportModel groupe, int startRow, string path, List<DateTime> dates)
        {
            foreach (var itemGroup in groupe.Scopes)
            {
                int startCol = 1;
                _excelWriter.WriteLine(sheet, startRow++, path, false, width: null, isTextWrap: null, ExcelHorizontalAlignment.Left,
                    new RowItem { Value = _converter?.ToScopesTableCategory(itemGroup.Key) ?? "", Col = startCol, FontColor = Constants.COLOR_BLACK, FontSize = Constants.FONT_SIZE_14, BgColor = Constants.COLOR_WHITE });

                foreach (var item in itemGroup.Value)
                {
                    List<RowItem> rowItems = new List<RowItem>
                        {
                        new RowItem { Value = item.WorkType, Col = startCol++, FontColor = Constants.COLOR_WHITE, FontSize = Constants.FONT_SIZE_14, BgColor = Constants.COLOR_DARK_BLUE },
                        new RowItem { Value = item.Price.ToString("N2"), Col = startCol++, FontColor = Constants.COLOR_WHITE, FontSize = Constants.FONT_SIZE_14,BgColor = Constants.COLOR_CORNFLOWER_BLUE },
                        new RowItem { Value = item.CompletedBeforeYear?.ToString("N2"), Col = startCol++, FontColor = Constants.COLOR_WHITE, FontSize = Constants.FONT_SIZE_14,BgColor = Constants.COLOR_CORNFLOWER_BLUE },
                        new RowItem { Value = item.Remaining?.ToString("N2"), Col = startCol++, FontColor = Constants.COLOR_WHITE, FontSize = Constants.FONT_SIZE_14,BgColor = Constants.COLOR_CORNFLOWER_BLUE },
                        new RowItem { Value = item.VolumeThisYear?.ToString("N2"), Col = startCol++, FontColor = Constants.COLOR_WHITE, FontSize = Constants.FONT_SIZE_14,BgColor = Constants.COLOR_CORNFLOWER_BLUE },
                        };

                    List<RowItem> periodsRow2 = new List<RowItem>();
                    foreach (var date in dates)
                    {
                        periodsRow2.Add(new RowItem
                        {
                            Value = item?.Costs?.FirstOrDefault(x => x.Period == date)?.Value?.ToString("N2") ?? "0,00",
                            Col = startCol++,
                            FontColor = Constants.COLOR_BLACK,
                            FontSize = Constants.FONT_SIZE_14,
                        });
                    }

                    rowItems.AddRange(periodsRow2);
                    _excelWriter.WriteLine(sheet, startRow++, path, false, width: null, isTextWrap: false, align: null, rowItems.ToArray());
                    startCol = 1;
                }
            }
            return startRow;
        }

        //public async Task<string> ExportContractDetails(int contractId, List<string> columnName)
        //{
        //    var contractDetails = _contract.Contracts
        //        .Find(x => x.Id == contractId)
        //        .Select(c => GetProperties(c, columnName));
        //    return await ExportContractDetailsExcelAsync(contractDetails);
        //}

        private async Task<string> ExportContractDetailsExcelAsync(IEnumerable<Dictionary<string, string>> contractDetails)
        {
            if (!Directory.Exists(_host.WebRootPath + "\\Temp"))
            {
                Directory.CreateDirectory(_host.WebRootPath + "\\Temp");
            }

            string path = _host.WebRootPath + "\\Temp" + "\\Детали_договора.xlsx";
            await Task.Run(() =>
            {
                var sheet = _excelWriter.Settup(path, "Детали договора");
                int startRow = 2;
                int startCol = 1;

                try
                {
                    ///ЗАПОЛНЯЕМ ШАПКУ ТАБЛИЦЫ
                    List<RowItem> rowItems = new List<RowItem>();
                    foreach (var colName in contractDetails.FirstOrDefault().Select(x => x.Key))
                    {
                        rowItems.Add(new RowItem
                        {
                            Value = _converter.ToRussianContractProps(colName),
                            Col = startCol++,
                            FontColor = Constants.COLOR_WHITE,
                            FontSize = Constants.FONT_SIZE_14,
                            BgColor = Constants.COLOR_DARK_BLUE
                        });
                    }
                    _excelWriter.WriteLine(sheet, startRow++, path, true, width: null, isTextWrap: null, ExcelHorizontalAlignment.Left, rowItems.ToArray());

                    ///ЗАПОЛНЯЕМ ТАБЛИЦУ
                    startCol = 1;
                    foreach (var detailValues in contractDetails.Select(x => x.Values))
                    {
                        List<RowItem> rowItems2 = new List<RowItem>();
                        foreach (var value in detailValues)
                        {
                            rowItems2.Add(new RowItem
                            {
                                Value = value,
                                Col = startCol++,
                                FontColor = Constants.COLOR_BLACK,
                                FontSize = Constants.FONT_SIZE_14,
                            });
                        }
                        _excelWriter.WriteLine(sheet, startRow, path, false, width: 25, isTextWrap: true, align: null, rowItems2.ToArray());
                        startCol = 1;
                        startRow++;
                    }

                    _excelWriter.CloseExcel();
                }
                catch (Exception)
                {
                    _excelWriter.CloseExcel();
                }
            });
            return path;
        }
    }
}
