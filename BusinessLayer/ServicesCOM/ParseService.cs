using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using BusinessLayer.Models.PRO;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.Reflection;

namespace BusinessLayer.ServicesCOM
{
    internal class ParseService : IParseService
    {
        private readonly IExcelReader _excelReader;
        private readonly IConverter _converter;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;
        private readonly ITextSearcher _textSearcher;
        private readonly IEstimateService _estimateService;
        public ParseService(IExcelReader excelReader, IConverter converter, ILoggerContract logger, IHttpContextAccessor http, 
            ITextSearcher textSearcher,IEstimateService estimateService)
        {
            _excelReader = excelReader;
            _converter = converter;
            _logger = logger;
            _http = http;
            _textSearcher = textSearcher;
            _estimateService = estimateService;
        }

        public ScopeWorkDTO GetScopeWorks(string path, int page)
        {
            var excel = _excelReader.GetExcelWorksheet(path, page);

            var scopeWork = new ScopeWorkDTO();
            var ending = excel.Dimension.End.Column;

            //вернет первое значение строку, второе - столбец
            var cellAboveDates = _excelReader.FindCellByQuery(excel, "в том числе по месяцам");
            int rowDates = cellAboveDates.FirstOrDefault().Item1 + 1;
            int startColumnOfDates = cellAboveDates.FirstOrDefault().Item2;


            var rowTotalSumSMR = _excelReader.FindCellByQuery(excel, "И Т О Г О СМР:", "итого смр", "итогосмр:");
            var rowTotalSumPNR = _excelReader.FindCellByQuery(excel, "И Т О Г О ПНР:", "итого пнр", "итогопнр:");

            var costs = new List<SWCost>();

            try
            {
                for (int i = startColumnOfDates; i <= ending; i++)
                {
                    var strDate = excel.Cells[rowDates, i].Value?.ToString().Trim();

                    DateTime costPeriod;
                    DateTime.TryParse(strDate, out costPeriod);

                    var valueSMR = _excelReader.GetValueDouble(excel, rowTotalSumSMR.FirstOrDefault().Item1, i);
                    var valuePNR = _excelReader.GetValueDouble(excel, rowTotalSumPNR.FirstOrDefault().Item1, i);

                    if (costPeriod == default(DateTime))
                    {
                        var date = _converter?.GetDateFromString(strDate);
                        if (date is not null)
                        {
                            costPeriod = (DateTime)date;
                        }
                    }

                    costs.Add(new SWCost
                    {
                        Period = costPeriod,
                        SmrCost = (decimal)valueSMR,
                        PnrCost = (decimal)valuePNR,
                    });
                }
                scopeWork.SWCosts = costs;

            }
            catch (Exception)
            {
            }

            return scopeWork;
        }

        public FormDTO Pars_C3A(string path, int page)
        {
            var excel = _excelReader.GetExcelWorksheet(path, page);
            var listString = new List<(string, int)>();
            var query = new List<List<string>>
            {
                new List<string>
                {
                    "стоимость выполненных строительно монтажных работ"
                },
                new List<string>
                {
                    "стоимость пусконаладочных работ"
                },
                new List<string>
                {
                    "стоимость дополнительн работ",
                    "стоимость доп работ"
                },
                new List<string>
                {
                    "сумма НДС по дополнительн работ",
                    "сумма НДС доп работ"
                },
                new List<string>
                {
                    "стоимость оборудования"
                },
                new List<string>
                {
                    "зачет целевого аванса"
                },
                new List<string>
                {
                    "зачет текущего аванса"
                },
                new List<string>
                {
                    "материалы"
                },
                new List<string>
                {
                    "возмещение стоимости",
                    "возврат стоимости"
                },
                new List<string>
                {
                    "другие"
                }
            };
            var c3A = new FormDTO
            {
                AdditionalCost = 0,
                EquipmentCost = 0,
                GenServiceCost = 0,
                MaterialCost = 0,
                OffsetCurrentPrepayment = 0,
                OffsetTargetPrepayment = 0,
                OtherExpensesCost = 0,
                PnrCost = 0,
                SmrCost = 0
            };

            var col = _excelReader.FindCellByQuery(excel, "за отчетный период").FirstOrDefault();
            if (col.Item2 == null) return null;

            for (var i = 0; i < query.Count; i++)
            {
                var find = _excelReader.FindCellByQuery(excel, query[i].ToArray());
                foreach (var item in find)
                {
                    (string, int) ob;
                    ob.Item1 = excel.Cells[item.Item1, item.Item2].Value.ToString();
                    ob.Item2 = item.Item1;
                    listString.Add(ob);
                }
            }

            var additionalWorkCoordates = listString.Where(x => _excelReader.FindByWords(x.Item1, "стоимость дополнительн работ", "стоимость доп работ")).ToList();
            var additionalNDSWorkCoordates = listString.Where(x => _excelReader.FindByWords(x.Item1, "сумма НДС дополнительн работ", "сумма НДС доп работ")).ToList();
            var smrCoordates = listString.Where(x => _excelReader.FindByWords(x.Item1, "стоимость выполненных строительно монтажных работ")).FirstOrDefault();
            var pnrCoordates = listString.Where(x => _excelReader.FindByWords(x.Item1, "стоимость пусконаладочных работ")).FirstOrDefault();
            var equipmentCoordates = listString.Where(x => _excelReader.FindByWords(x.Item1, "Стоимость оборудования")).FirstOrDefault();

            if (smrCoordates.Item1 != null)
            {
                var additionalWork = 0M;
                foreach (var item in additionalWorkCoordates)
                {
                    if (item.Item2 > smrCoordates.Item2 && item.Item2 < pnrCoordates.Item2 ||
                        item.Item2 > smrCoordates.Item2 && item.Item2 < equipmentCoordates.Item2 && pnrCoordates.Item2 == null)
                    {
                        additionalWork += (decimal)_excelReader.GetValueDouble(excel, item.Item2, col.Item2);
                        break;
                    }
                }
                foreach (var item in additionalNDSWorkCoordates)
                {
                    if (item.Item2 > smrCoordates.Item2 && item.Item2 < pnrCoordates.Item2 ||
                        item.Item2 > smrCoordates.Item2 && item.Item2 < equipmentCoordates.Item2 && pnrCoordates.Item2 == null)
                    {
                        additionalWork += (decimal)_excelReader.GetValueDouble(excel, item.Item2, col.Item2);
                        break;
                    }
                }

                var smrCost = (decimal)_excelReader.GetValueDouble(excel, smrCoordates.Item2, col.Item2);
                smrCost -= additionalWork;
                c3A.AdditionalCost += additionalWork;
                c3A.FixedContractPrice = smrCost;
            }
            if (pnrCoordates.Item1 != null)
            {
                var additionalWork = 0M;
                foreach (var item in additionalWorkCoordates)
                {
                    if (item.Item2 > pnrCoordates.Item2 && item.Item2 < equipmentCoordates.Item2)
                    {
                        additionalWork += (decimal)_excelReader.GetValueDouble(excel, item.Item2, col.Item2);
                        break;
                    }
                }
                foreach (var item in additionalNDSWorkCoordates)
                {
                    if (item.Item2 > pnrCoordates.Item2 && item.Item2 < equipmentCoordates.Item2)
                    {
                        additionalWork += (decimal)_excelReader.GetValueDouble(excel, item.Item2, col.Item2);
                        break;
                    }
                }
                var pnrCost = (decimal)_excelReader.GetValueDouble(excel, pnrCoordates.Item2, col.Item2);
                pnrCost -= additionalWork;
                c3A.AdditionalCost += additionalWork;
                c3A.PnrCost = pnrCost;
            }
            c3A.EquipmentCost = (decimal)_excelReader.GetValueDouble(excel,
                listString.Where(x => _excelReader.FindByWords(x.Item1, "Стоимость оборудования")).FirstOrDefault().Item2, col.Item2);
            c3A.OffsetTargetPrepayment = (decimal)_excelReader.GetValueDouble(excel,
                listString.Where(x => _excelReader.FindByWords(x.Item1, "зачет целевого аванса")).FirstOrDefault().Item2, col.Item2);
            c3A.OffsetCurrentPrepayment = (decimal)_excelReader.GetValueDouble(excel,
                listString.Where(x => _excelReader.FindByWords(x.Item1, "зачет текущего аванса")).FirstOrDefault().Item2, col.Item2);
            c3A.MaterialCost = (decimal)_excelReader.GetValueDouble(excel,
                listString.Where(x => _excelReader.FindByWords(x.Item1, "материалы")).FirstOrDefault().Item2, col.Item2);
            c3A.GenServiceCost = (decimal)_excelReader.GetValueDouble(excel,
                listString.Where(x => _excelReader.FindByWords(x.Item1, "другие генуслуги")).FirstOrDefault().Item2, col.Item2);
            c3A.OtherExpensesCost = 0;
            foreach (var item in listString.Where(x => _excelReader.FindByWords(x.Item1, "другие", "возмещение стоимости", "возврат стоимости")))
            {
                if (!(_excelReader.FindByWords(item.Item1, "другие генуслуги")))
                {
                    c3A.OtherExpensesCost += (decimal)_excelReader.GetValueDouble(excel,
                    item.Item2, col.Item2);
                }
            }
            return c3A;
        }

        public EstimateDTO ParseEstimate(string path, int page)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            var estimate = new EstimateDTO();
            try
            {
                var excel = _excelReader.GetExcelWorksheet(path, page);
                var ending = excel.Dimension.End.Column;

                estimate.BuildingName = GetCellValue(excel, shiftRow: 0, shiftCol: 1, "Наименование здания, сооружения", "НАИМЕНОВАНИЕ ЗДАНИЯ, СООРУЖЕНИЯ");
                estimate.BuildingCode = GetCellValue(excel, shiftRow: 0, shiftCol: 1, "Шифр здания, сооружения", "ШИФР ЗДАНИЯ, СООРУЖЕНИЯ");
                estimate.DrawingsKit = GetCellValue(excel, shiftRow: 0, shiftCol: 1, "КОМПЛЕКТ ЧЕРТЕЖЕЙ", "Комплект чертежей");
                var Number = GetCellValue(excel, shiftRow: 0, shiftCol: 0, "Локальная смета", "ЛОКАЛЬНАЯ СМЕТА", "Локальная смета (Локальный сметный расчет)");

                estimate.Number = _textSearcher?.FindEstimateNumber(Number) ?? "";

                //TODO: пересмотреть поиск по всем столбцам, может быть сдвиги

                var cellAboveDates5 = _excelReader.FindCellByQuery(excel, "Составлена в ценах на", "Составлена в", "СОСТАВЛЕНА В");
                int rowNameEstimate = cellAboveDates5.FirstOrDefault().Item1 - 1;
                var drawingName = string.Empty;

                for (int i = excel.Dimension.Start.Column; i <= excel.Dimension.End.Column; i++)
                {
                    drawingName = excel.Cells[rowNameEstimate, i].Value?.ToString()?.Trim();
                    if (drawingName is not null)
                    {
                        break;
                    }
                    if (i == excel.Dimension.End.Column && string.IsNullOrEmpty(drawingName))
                    {
                        i = excel.Dimension.Start.Column;
                        rowNameEstimate = rowNameEstimate - 1;
                    }
                }
                estimate.DrawingsName = drawingName;
            }
            catch (Exception e)
            {
                _logger.WriteLog(
                               logLevel: LogLevel.Warning,
                               message: e.Message,
                               nameSpace: typeof(ParseService).Name,
                               methodName: MethodBase.GetCurrentMethod().Name,
                               userName: user
                               );
            }
            return estimate;
        }

        public void ParseAndReturnLaborCosts(string path, int page, int estimateId)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";
            
            var estimate = _estimateService.GetById(estimateId);

            var numberEstimate = estimate.BuildingCode + "." + estimate.Number;
            try
            {
                var excel = _excelReader.GetExcelWorksheet(path, page);
                //var ending = excel.Dimension.End.Column;              
                var nameColumnLabor = _excelReader.FindCellByQuery(excel, "трудозатраты", "трудозатрат", "трудозатраты чел.час.", "ТРУДОЗАТРАТ");
                var OneofNumber = _excelReader.FindCellByQuery(excel, estimate.DrawingsName).FirstOrDefault();
                var num = excel.Cells[OneofNumber.Item1, OneofNumber.Item2].Value?.ToString() ?? "";
                num = num.Substring(0, num.ToLower().IndexOf(estimate.DrawingsName.ToLower().ElementAt(0))-1).Trim();
                if (num.Last() == '.') num = num.Substring(0, num.Count() - 1);
                var nameRowEstimate = _excelReader.FindCellByQuery(excel, $"И Т О Г О по смете {num}", $"ИТОГО по смете {num}", $"Итого по смете {num}");
                var col = nameColumnLabor.FirstOrDefault().Item2;
                var row = nameRowEstimate.FirstOrDefault().Item1;
                var result = excel.Cells[row, col].Value ?? 0;

                estimate.LaborCost = (double)result;
                _estimateService.Update(estimate);

            }
            catch (Exception e)
            { 
                _logger.WriteLog(
                               logLevel: LogLevel.Warning,
                               message: e.Message,
                               nameSpace: typeof(ParseService).Name,
                               methodName: MethodBase.GetCurrentMethod().Name,
                               userName: user
                               );
            }           
        }

        public void ParseAndReturnContractCosts(string path, int page, int estimateId)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";
            
            var estimate = _estimateService.GetById(estimateId);

            var numberEstimate = estimate.BuildingCode + "." + estimate.Number;
            try
            {
                var excel = _excelReader.GetExcelWorksheet(path, page);                
                var nameColumnLabor = _excelReader.FindCellByQuery(excel, "всего");
                var OneofNumber = _excelReader.FindCellByQuery(excel, estimate.DrawingsName).FirstOrDefault();
                var num = excel.Cells[OneofNumber.Item1, OneofNumber.Item2].Value?.ToString() ?? "";
                num = num.Substring(0, num.ToLower().IndexOf(estimate.DrawingsName.ToLower().ElementAt(0)) - 1).Trim();
                if (num.Last() == '.') num = num.Substring(0, num.Count() - 1);
                var nameRowEstimate = _excelReader.FindCellByQuery(excel, $"И Т О Г О по смете {num}", $"ИТОГО по смете {num}", $"Итого по смете {num}");
                var col = nameColumnLabor.FirstOrDefault().Item2;
                var row = nameRowEstimate.FirstOrDefault().Item1;
                var result = excel.Cells[row, col].Value ?? 0;

                estimate.ContractsCost = (decimal)result;
                _estimateService.Update(estimate);

            }
            catch (Exception e)
            {
                _logger.WriteLog(
                               logLevel: LogLevel.Warning,
                               message: e.Message,
                               nameSpace: typeof(ParseService).Name,
                               methodName: MethodBase.GetCurrentMethod().Name,
                               userName: user
                               );
            }
        }

        public void ParseAndReturnDoneSmrCost(string path, int page, int estimateId)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            var estimate = _estimateService.GetById(estimateId);

            var numberEstimate = estimate.BuildingCode + "." + estimate.Number;
            try
            {
                var excel = _excelReader.GetExcelWorksheet(path, page);
                var nameColumnLabor = _excelReader.FindCellByQuery(excel, "с начала строительства");
                var OneofNumber = _excelReader.FindCellByQuery(excel, estimate.DrawingsName).FirstOrDefault();
                var num = excel.Cells[OneofNumber.Item1, OneofNumber.Item2].Value?.ToString() ?? "";
                num = num.Substring(0, num.ToLower().IndexOf(estimate.DrawingsName.ToLower().ElementAt(0)) - 1).Trim();
                if (num.Last() == '.') num = num.Substring(0, num.Count() - 1);
                var nameRowEstimate = _excelReader.FindCellByQuery(excel, $"в с е г о по смете {num}", $"всего по смете {num}");
                var col = nameColumnLabor.FirstOrDefault().Item2;
                var row = nameRowEstimate.FirstOrDefault().Item1;
                var result = excel.Cells[row, col+1].Value ?? 0;

                estimate.DoneSmrCost = (decimal)result;
                _estimateService.Update(estimate);

            }
            catch (Exception e)
            {
                _logger.WriteLog(
                               logLevel: LogLevel.Warning,
                               message: e.Message,
                               nameSpace: typeof(ParseService).Name,
                               methodName: MethodBase.GetCurrentMethod().Name,
                               userName: user
                               );
            }
        }

        private string GetCellValue(ExcelWorksheet excel, int shiftRow, int shiftCol, params string[] names)
        {
            //вернет первое значение строку, второе - столбец
            var cell = _excelReader.FindCellByQuery(excel, names);
            int rowDates = cell.FirstOrDefault().Item1 + shiftRow;
            int startColumnOfDates = cell.FirstOrDefault().Item2 + shiftCol;
            return excel.Cells[rowDates, startColumnOfDates].Value?.ToString()?.Trim() ?? string.Empty;
        }
    }
}
