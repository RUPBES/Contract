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
            ITextSearcher textSearcher, IEstimateService estimateService)
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
                    "договор цен НДС"
                },
                new List<string>
                {
                    "сумма НДС"
                },
                new List<string>
                {
                    "НДС"
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
                    "стоимость оборудования ндс"
                },
                new List<string>
                {
                    "стоимость оборудования ндс транспорт"
                },
                new List<string>
                {
                    "оборудован заказчик"
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
                    "материал подрядчик"
                },
                new List<string>
                {
                    "материал заказчик"
                },
                new List<string>
                {
                    "возмещение стоимости",
                    "возврат стоимости"
                },
                new List<string>
                {
                    "другие"
                },
                new List<string>
                {
                    "средств фонд"
                },
                new List<string>
                {
                    "стоимость работ отчет",
                    "стоимость работ отчёт"
                },
                new List<string>
                {
                    "сумм учитыв расчет вып раб"
                }
            };
            var c3A = new FormDTO
            {
                AdditionalCost = 0,
                AdditionalContractCost = 0,
                AdditionalNdsCost = 0,
                EquipmentCost = 0,
                EquipmentClientCost = 0,
                EquipmentContractCost = 0,
                EquipmentNdsCost = 0,
                GenServiceCost = 0,
                MaterialCost = 0,
                MaterialClientCost = 0,
                OffsetCurrentPrepayment = 0,
                OffsetTargetPrepayment = 0,
                OtherExpensesCost = 0,
                OtherExpensesNdsCost = 0,
                PnrCost = 0,
                PnrContractCost = 0,
                PnrNdsCost = 0,
                SmrCost = 0,
                SmrContractCost = 0,
                SmrNdsCost = 0,
                CostToConstructionIndustryFund = 0,
                CostStatisticReportOfContractor = 0
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
                    if (!listString.Contains(ob))
                    listString.Add(ob);
                }
            }

            var additionalWorkCoordates = listString.Where(x => _excelReader.FindByWords(x.Item1, "стоимость дополнительн работ", "стоимость доп работ")).FirstOrDefault();
            var additionalNDSWorkCoordates = listString.Where(x => _excelReader.FindByWords(x.Item1, "сумма НДС дополнительн работ", "сумма НДС доп работ")).FirstOrDefault();

            var smrAllCoordates = listString.Where(x => _excelReader.FindByWords(x.Item1, "стоимость выполненных строительно монтажных работ")).FirstOrDefault();
            var SmrPnrContractCoordates = listString.Where(x => _excelReader.FindByWords(x.Item1, "договор цен НДС")).ToList();

            var pnrCoordates = listString.Where(x => _excelReader.FindByWords(x.Item1, "стоимость пусконаладочных работ")).FirstOrDefault();

            var equipmentCoordates = listString.Where(x => _excelReader.FindByWords(x.Item1, "стоимость оборудования ндс")).FirstOrDefault();            
            var equipmentTransportCoordates = listString.Where(x => _excelReader.FindByWords(x.Item1, "стоимость оборудования ндс транспорт")).FirstOrDefault();
            var equipmentClientCoordates = listString.Where(x => _excelReader.FindByWords(x.Item1, "оборудован заказчик")).FirstOrDefault();

            var sumWorkCoordates = listString.Where(x => _excelReader.FindByWords(x.Item1, "сумм учитыв расчет вып раб")).FirstOrDefault();

            var statistic  = listString.Where(x => _excelReader.FindByWords(x.Item1, "стоимость работ отчет", "стоимость работ отчёт")).FirstOrDefault();

            var NdsPrice = listString.Where(x => _excelReader.FindByWords(x.Item1, "сумма НДС")).ToList();
            var Nds = listString.Where(x => _excelReader.FindByWords(x.Item1, "НДС")).ToList();

            c3A.AdditionalContractCost += (decimal)_excelReader.GetValueDouble(excel, additionalWorkCoordates.Item2, col.Item2);
            c3A.AdditionalNdsCost += (decimal)_excelReader.GetValueDouble(excel, additionalNDSWorkCoordates.Item2, col.Item2);
            if (statistic.Item1 != null)
            {
                c3A.CostStatisticReportOfContractor = (decimal)_excelReader.GetValueDouble(excel, statistic.Item2, col.Item2);
            }

            foreach (var item in SmrPnrContractCoordates)
            {
                if (pnrCoordates.Item1 != null && item.Item2 > pnrCoordates.Item2)
                {
                    c3A.PnrContractCost = (decimal)_excelReader.GetValueDouble(excel, item.Item2, col.Item2);
                }
                else if (item.Item2 != additionalWorkCoordates.Item2)
                {
                    c3A.SmrContractCost = (decimal)_excelReader.GetValueDouble(excel, item.Item2, col.Item2);
                }
            }

            foreach (var item in NdsPrice)
            {
                if (pnrCoordates.Item1 != null)
                {
                    if (item.Item2 < pnrCoordates.Item2 && item.Item2 != additionalNDSWorkCoordates.Item2)
                        c3A.SmrNdsCost = (decimal)_excelReader.GetValueDouble(excel, item.Item2, col.Item2);
                    else
                    if (item.Item2 > pnrCoordates.Item2 && item.Item2 < equipmentCoordates.Item2)
                    {
                        c3A.PnrNdsCost = (decimal)_excelReader.GetValueDouble(excel, item.Item2, col.Item2);
                    }
                    else 
                    if (item.Item2 < sumWorkCoordates.Item2)
                    {
                        c3A.EquipmentNdsCost = (decimal)_excelReader.GetValueDouble(excel, item.Item2, col.Item2);
                    }
                }
                else
                {
                    if (item.Item2 < equipmentCoordates.Item2)
                    {
                        c3A.SmrNdsCost = (decimal)_excelReader.GetValueDouble(excel, item.Item2, col.Item2);
                    }
                    else
                    {
                        c3A.EquipmentNdsCost = (decimal)_excelReader.GetValueDouble(excel, item.Item2, col.Item2);
                    }
                }
            }

            foreach (var item in Nds)
            {
                if (item.Item2 > sumWorkCoordates.Item2)
                {
                    c3A.OtherExpensesNdsCost += (decimal)_excelReader.GetValueDouble(excel, item.Item2, col.Item2);
                }
                
            }

            if (equipmentTransportCoordates.Item1 != null)
            {
                c3A.EquipmentContractCost = (decimal)_excelReader.GetValueDouble(excel, equipmentTransportCoordates.Item2, col.Item2);
            }
            else
            {
                c3A.EquipmentContractCost = (decimal)_excelReader.GetValueDouble(excel, equipmentCoordates.Item2, col.Item2);
            }

            c3A.EquipmentClientCost = (decimal)_excelReader.GetValueDouble(excel, equipmentClientCoordates.Item2, col.Item2);
            c3A.OffsetTargetPrepayment = (decimal)_excelReader.GetValueDouble(excel,
                listString.Where(x => _excelReader.FindByWords(x.Item1, "зачет целевого аванса")).FirstOrDefault().Item2, col.Item2);
            c3A.OffsetCurrentPrepayment = (decimal)_excelReader.GetValueDouble(excel,
                listString.Where(x => _excelReader.FindByWords(x.Item1, "зачет текущего аванса")).FirstOrDefault().Item2, col.Item2);
            c3A.MaterialCost = (decimal)_excelReader.GetValueDouble(excel,
                listString.Where(x => _excelReader.FindByWords(x.Item1, "материал подрядчик")).FirstOrDefault().Item2, col.Item2);
            c3A.MaterialClientCost = (decimal)_excelReader.GetValueDouble(excel,
                listString.Where(x => _excelReader.FindByWords(x.Item1, "материал заказчик")).FirstOrDefault().Item2, col.Item2);
            c3A.GenServiceCost = (decimal)_excelReader.GetValueDouble(excel,
                listString.Where(x => _excelReader.FindByWords(x.Item1, "другие генуслуги")).FirstOrDefault().Item2, col.Item2);
            c3A.CostToConstructionIndustryFund = (decimal)_excelReader.GetValueDouble(excel,
                listString.Where(x => _excelReader.FindByWords(x.Item1, "средств фонд")).FirstOrDefault().Item2, col.Item2);
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
                if (GetCellValue(excel, shiftRow: 0, shiftCol: 0, "Локальная смета", "Локальный сметный расчет").Count() < 1)
                {
                    throw new Exception("Файл не является локальной сметой");
                }
                estimate.BuildingName = GetCellValue(excel, shiftRow: 0, shiftCol: 1, "Наименование здания, сооружения", "НАИМЕНОВАНИЕ ЗДАНИЯ, СООРУЖЕНИЯ");
                estimate.BuildingCode = GetCellValue(excel, shiftRow: 0, shiftCol: 1, "Шифр здания, сооружения", "ШИФР ЗДАНИЯ, СООРУЖЕНИЯ");
                estimate.DrawingsKit = GetCellValue(excel, shiftRow: 0, shiftCol: 1, "КОМПЛЕКТ ЧЕРТЕЖЕЙ", "Комплект чертежей");
                var Number = GetCellValue(excel, shiftRow: 0, shiftCol: 0, "Локальная смета", "ЛОКАЛЬНАЯ СМЕТА", "Локальная смета (Локальный сметный расчет)");

                estimate.Number = _textSearcher?.FindEstimateNumber(Number) ?? "";

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
                return null;
            }
            return estimate;
        }

        public bool ParseAndReturnLaborCosts(string path, int page, int estimateId)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            var estimate = _estimateService.GetById(estimateId);

            var numberEstimate = estimate.BuildingCode + "." + estimate.Number;
            try
            {
                var excel = _excelReader.GetExcelWorksheet(path, page);
                if (GetCellValue(excel, shiftRow: 0, shiftCol: 0, "Расчет стоимости").Count() < 1)
                {
                    throw new Exception("Файл не является расчетом стоимости");
                }
                var nameColumnLabor = _excelReader.FindCellByQuery(excel, "трудозатраты", "трудозатрат", "трудозатраты чел.час.", "ТРУДОЗАТРАТ");
                var OneofNumber = _excelReader.FindCellByQuery(excel, estimate.DrawingsName).FirstOrDefault();
                var num = excel.Cells[OneofNumber.Item1, OneofNumber.Item2].Value?.ToString() ?? "";
                num = num.Substring(0, num.ToLower().IndexOf(estimate.DrawingsName.ToLower().ElementAt(0)) - 1).Trim();
                if (num.Last() == '.') num = num.Substring(0, num.Count() - 1);
                var nameRowEstimate = _excelReader.FindCellByQuery(excel, $"И Т О Г О по смете {num}", $"ИТОГО по смете {num}", $"Итого по смете {num}");
                var col = nameColumnLabor.FirstOrDefault().Item2;
                var row = nameRowEstimate.FirstOrDefault().Item1;
                var result = excel.Cells[row, col].Value ?? 0;

                estimate.LaborCost = (double)result;
                _estimateService.Update(estimate);
                return true;
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
                return false;
            }
        }

        public bool ParseAndReturnContractCosts(string path, int page, int estimateId)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            var estimate = _estimateService.GetById(estimateId);

            var numberEstimate = estimate.BuildingCode + "." + estimate.Number;
            try
            {
                var excel = _excelReader.GetExcelWorksheet(path, page);
                if (GetCellValue(excel, shiftRow: 0, shiftCol: 0, "График строительства").Count() < 1)
                {
                    throw new Exception("Файл не является графиком строительства");
                }
                var nameColumnLabor = _excelReader.FindCellByQuery(excel, "всего");
                var OneofNumber = _excelReader.FindCellByQuery(excel, estimate.DrawingsName).FirstOrDefault();
                var num = excel.Cells[OneofNumber.Item1, OneofNumber.Item2].Value?.ToString() ?? "";
                num = num.Substring(0, num.ToLower().IndexOf(estimate.DrawingsName.ToLower().ElementAt(0)) - 1).Trim();
                if (num.Last() == '.') num = num.Substring(0, num.Count() - 1);
                var nameRowEstimate = _excelReader.FindCellByQuery(excel, $"И Т О Г О по смете {num}", $"ИТОГО по смете {num}", $"Итого по смете {num}");
                var col = nameColumnLabor.FirstOrDefault().Item2;
                var row = nameRowEstimate.FirstOrDefault().Item1;
                var result = excel.Cells[row, col].Value ?? 0M;

                estimate.ContractsCost = decimal.Parse(result.ToString().Replace('.', ','));
                _estimateService.Update(estimate);
                return true;
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
                return false;
            }
        }

        public bool ParseAndReturnDoneSmrCost(string path, int page, int estimateId)
        {
            var name = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "given_name")?.Value ?? null;
            var family = _http?.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "family_name")?.Value ?? null;
            var user = (name != null || family != null) ? ($"{family} {name}") : "Не определен";

            var estimate = _estimateService.GetById(estimateId);
            try
            {
                var excel = _excelReader.GetExcelWorksheet(path, page);
                if (GetCellValue(excel, shiftRow: 0, shiftCol: 0, "СДАЧИ - ПРИЕМКИ ВЫПОЛНЕННЫХ СТРОИТЕЛЬНЫХ И ИНЫХ СПЕЦИАЛЬНЫХ МОНТАЖНЫХ РАБОТ").Count() < 1)
                {
                    throw new Exception("Файл не является справкой С-2б");
                }
                var nameColumnLabor = _excelReader.FindCellByQuery(excel, "с начала строительства");
                var nameRowEstimate = _excelReader.FindCellByQuery(excel, $"в с е г о по смете №{estimate.Number}", $"всего по смете №{estimate.Number}");
                var col = nameColumnLabor.FirstOrDefault().Item2 + 1;
                var row = nameRowEstimate.FirstOrDefault().Item1;
                var result = excel.Cells[row, col].Value ?? 0;

                estimate.DoneSmrCost = decimal.Parse(result.ToString().Replace('.', ','));
                _estimateService.Update(estimate);
                return true;
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
                return false;
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
