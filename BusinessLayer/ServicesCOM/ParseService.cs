using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using BusinessLayer.Models.PRO;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.Models.PRO;
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
        private readonly ITextSearcher _textSearcher;
        private readonly IEstimateService _estimateService;
        private readonly IHttpHelper _httpHelper;
        public ParseService(IExcelReader excelReader, IConverter converter, ILoggerContract logger,
            ITextSearcher textSearcher, IEstimateService estimateService, IHttpHelper httpHelper)
        {
            _excelReader = excelReader;
            _converter = converter;
            _logger = logger;
            _textSearcher = textSearcher;
            _estimateService = estimateService;
            _httpHelper = httpHelper;
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

            var statistic = listString.Where(x => _excelReader.FindByWords(x.Item1, "стоимость работ отчет", "стоимость работ отчёт")).FirstOrDefault();

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

        public EstimateDTO ParseEstimate(string path, int page, string? type)
        {
            var estimate = new EstimateDTO();
            try
            {
                var searchingKeys = _estimateService.ReturnKeysSearch(type);
                var excel = _excelReader.GetExcelWorksheet(path, page);
                string estimateNumber = string.Empty;
                if (type == ConstantsApp.SXW_SINKEVICH_APP)
                {
                    estimateNumber = GetCellValue(excel, shiftRow: 0, shiftCol: 1, searchingKeys.Estimate.DocName.ToArray());
                    estimate.Number = estimateNumber ?? string.Empty;
                }
                else
                {
                    estimateNumber = GetCellValue(excel, shiftRow: 0, shiftCol: 0, searchingKeys.Estimate.DocName.ToArray());
                    estimate.Number = _textSearcher?.SearchNumberWithEnd(estimateNumber) ?? "";
                }
                if (estimateNumber == string.Empty)
                {
                    throw new Exception("Файл не является локальной сметой");
                }


                var BuildingName = _excelReader.FindCellByQuery(excel, searchingKeys.Estimate.BuildingName.ToArray());
                if (BuildingName.Count() > 0)
                {
                    estimate.BuildingName = GetValueInRow(excel, BuildingName.FirstOrDefault().Item1, BuildingName.FirstOrDefault().Item2);
                }


                var BuildingCode = _excelReader.FindCellByQuery(excel, searchingKeys.Estimate.BuildingCode.ToArray());
                if (BuildingCode.Count() > 0)
                {
                    estimate.BuildingCode = GetValueInRow(excel, BuildingCode.FirstOrDefault().Item1, BuildingCode.FirstOrDefault().Item2);
                }

                var DrawingsKit = _excelReader.FindCellByQuery(excel, searchingKeys.Estimate.DrawingKit.ToArray());
                if (DrawingsKit.Count() > 0)
                {
                    estimate.DrawingsKit = GetValueInRow(excel, DrawingsKit.FirstOrDefault().Item1, DrawingsKit.FirstOrDefault().Item2);
                }
                var drawingName = string.Empty;
                var cellAboveDates = _excelReader.FindCellByQuery(excel, searchingKeys.Estimate.StartLineLookingForEstimateName.ToArray());
                if (cellAboveDates.Count() > 0)
                {
                    int rowNameEstimate = cellAboveDates.FirstOrDefault().Item1 - 1;


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

                    estimate.DrawingsName = drawingName?.Replace("На ", "")?.Replace("НА ", "").Replace("на ", "");
                }
            }
            catch (Exception e)
            {
                _logger.WriteLog(logLevel: LogLevel.Warning, message: e.Message, nameSpace: typeof(ParseService).Name,
                                 methodName: MethodBase.GetCurrentMethod().Name);
                return null;
            }
            return estimate;
        }

        /// <summary>
        /// ДЛЯ ОДНОЙ СМЕТЫ
        /// -- Ищет в одном документе, в конкретной странице стоимость одной сметы, которая находится по estimateId
        /// </summary>
        /// <param name="path">путь к Excel</param>
        /// <param name="page">страница Excel, где искать трудозатраты</param>
        /// <param name="estimateId">Сметы ID, чтобы взять номер сметы</param>
        /// <param name="type">Тип программного комплекса, откуда извлечен Excel</param>
        /// <returns></returns>
        /// <exception cref="Exception">Если эта страница (PAGE), не содержит ключевое название 
        /// документа (Расчет стоимости или Ведомость..), выбрасывает ошибку, </exception>
        public bool ParseAndReturnLaborCosts(string path, int page, int estimateId, string? type)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            var estimate = _estimateService.GetById(estimateId);
            var searchingKeys = _estimateService.ReturnKeysSearch(type);
            var excel = _excelReader.GetExcelWorksheet(path, page);
            var shiftRow = 0;

            if (type != ConstantsApp.SMR_PRO_APP)
            {
                shiftRow += 2;
            }

            if (GetCellValue(excel, shiftRow: 0, shiftCol: 0, searchingKeys.LaborCost.DocName.ToArray()) == string.Empty)
            {
                throw new Exception("Файл не является расчетом стоимости");
            }
            if (ExistEstmtNumberIntoSheet(excel, estimate.DrawingsName, estimate?.FullNumber))
            {
                throw new Exception($"Данный расчет стоимости не содержит строку с сметой №{estimate?.FullNumber}");
            }

            var arrayColumnCoordinates = _excelReader.FindCellByQuery(excel, searchingKeys.LaborCost.ColName.ToArray());



            estimate.LaborCost = (double)ReturnResultSearching(excel, arrayColumnCoordinates, searchingKeys.LaborCost.RowName, estimate?.FullNumber, shiftRow);
            _estimateService.Update(estimate);

            return true;
        }

        public bool ParseAndReturnContractCosts(string path, int page, int estimateId, string? type)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            var estimate = _estimateService.GetById(estimateId);
            var searchingKeys = _estimateService.ReturnKeysSearch(type);
            var excel = _excelReader.GetExcelWorksheet(path, page);
            var newDrawing = string.Empty;
            if (estimate.DrawingsName.Length > 17)
            {
                newDrawing = estimate.DrawingsName.Substring(0, 20);
            }
            if (GetCellValue(excel, shiftRow: 0, shiftCol: 0, searchingKeys.ContractCost.DocName.ToArray()) == string.Empty)
            {
                throw new Exception("Файл не является графиком строительства");
            }
            if (ExistEstmtNumberIntoSheet(excel, newDrawing, estimate.FullNumber))
            {
                throw new Exception($"Данный график строительства не содержит строку с сметой №{estimate?.FullNumber}");
            }

            var arrayColumnCoordinates = _excelReader.FindCellByQuery(excel, searchingKeys.ContractCost.ColName.ToArray());
            estimate.ContractsCost = ReturnResultSearching(excel, arrayColumnCoordinates, searchingKeys.ContractCost.RowName, estimate?.FullNumber, 0);
            _estimateService.Update(estimate);

            return true;
        }

        public bool ParseAndReturnDoneSmrCost(string path, int page, int estimateId, string? type)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            var estimate = _estimateService.GetById(estimateId);
            var searchingKeys = _estimateService.ReturnKeysSearch(type);
            var excel = _excelReader.GetExcelWorksheet(path, page);

            if (GetCellValue(excel, shiftRow: 0, shiftCol: 0, searchingKeys.DoneSmrCost.DocName.ToArray()) == string.Empty)
            {
                throw new Exception("Файл не является справкой С-2б");
            }

            var shiftRow = 0;
            if (type == ConstantsApp.SXW_SINKEVICH_APP)
            {
                shiftRow += 1;
            }
            var arrayColumnCoordinates = _excelReader.FindCellByQuery(excel, searchingKeys.DoneSmrCost.ColName.ToArray());
            estimate.DoneSmrCost = ReturnResultSearching(excel, arrayColumnCoordinates,
                                                          searchingKeys.DoneSmrCost.RowName, estimate?.FullNumber, shiftRow,
                                                          searchingKeys.DoneSmrCost.ExtraColName.FirstOrDefault(),
                                                          searchingKeys.DoneSmrCost.ExtraRowName);

            _estimateService.Update(estimate);
            return true;
        }


        /// <summary>
        /// ДЛЯ МНОГО СМЕТ НА ОДНОЙ СТРАНИЦЕ
        /// -- Ищет в Excel, по определенной странице, с заглавием содержащее ключевое слово (Расчет стоимости или Ведомость..), 
        ///  стоимость трудоемкости с номером сметы и возвращает массив картежей (номер сметы, стоимость трудоемкости работ)
        /// </summary>
        /// <param name="path">путь к Excel</param>
        /// <param name="page">страница Excel, где искать трудозатраты</param>
        /// <param name="type">Тип программного комплекса, откуда извлечен Excel</param>
        /// <returns>Возвращает массив картежей (номер сметы, стоимость трудоемкости работ)</returns>
        public List<(string estimateNumber, decimal cost)>? GetEstimatesWithLaborCosts(string path, int page, string? type)
        {
            if (string.IsNullOrEmpty(path))
            {
                return Array.Empty<(string, decimal)>().ToList();
            }

            var searchingKeys = _estimateService.ReturnKeysSearch(type);
            var excel = _excelReader.GetExcelWorksheet(path, page);
            var shiftRow = 0;

            if (type != ConstantsApp.SMR_PRO_APP)
            {
                shiftRow += 2;
            }

            if (GetCellValue(excel, shiftRow: 0, shiftCol: 0, searchingKeys.LaborCost.DocName.ToArray()) == string.Empty)
            {
                return Array.Empty<(string, decimal)>().ToList();
            }
            var column = _excelReader.FindCellByQuery(excel, searchingKeys.LaborCost.ColName.ToArray())?.FirstOrDefault();

            if (column is null)
            {
                return Array.Empty<(string, decimal)>().ToList();
            }
            return ReturnListEstNumbersWithCosts(excel, (column?.Item2 ?? 0), searchingKeys.LaborCost.RowName, shiftRow);
        }

        /// <summary>
        /// ДЛЯ МНОГО СМЕТ НА ОДНОЙ СТРАНИЦЕ
        /// -- Ищет в Excel, по определенной странице, с заглавием содержащее ключевое слово (Расчет стоимости или Ведомость..), 
        ///  стоимость трудоемкости с номером сметы и возвращает массив картежей (номер сметы, стоимость трудоемкости работ)
        /// </summary>
        /// <param name="path">путь к Excel</param>
        /// <param name="page">страница Excel, где искать трудозатраты</param>
        /// <param name="type">Тип программного комплекса, откуда извлечен Excel</param>
        /// <returns>Возвращает массив картежей (номер сметы, стоимость трудоемкости работ)</returns>
        public List<(string estimateNumber, decimal cost)>? GetEstimatesWithContractCosts(string path, int page, string? type)
        {
            if (string.IsNullOrEmpty(path))
            {
                return Array.Empty<(string, decimal)>().ToList();
            }

            var searchingKeys = _estimateService.ReturnKeysSearch(type);
            var excel = _excelReader.GetExcelWorksheet(path, page);

            if (GetCellValue(excel, shiftRow: 0, shiftCol: 0, searchingKeys.ContractCost.DocName.ToArray()) == string.Empty)
            {
                return Array.Empty<(string, decimal)>().ToList();
            }

            var columnCoordinate = _excelReader.FindCellByQuery(excel, searchingKeys.ContractCost.ColName.ToArray())?.FirstOrDefault();
            return ReturnListEstNumbersWithCosts(excel, (columnCoordinate?.Col ?? 0), searchingKeys.ContractCost.RowName, shiftRow: 0);

        }

        /// <summary>
        /// ДЛЯ МНОГО СМЕТ НА ОДНОЙ СТРАНИЦЕ
        /// -- Ищет в Excel, по определенной странице, с заглавием содержащее ключевое слово (Расчет стоимости или Ведомость..), 
        ///  стоимость трудоемкости с номером сметы и возвращает массив картежей (номер сметы, стоимость трудоемкости работ)
        /// </summary>
        /// <param name="path">путь к Excel</param>
        /// <param name="page">страница Excel, где искать трудозатраты</param>
        /// <param name="type">Тип программного комплекса, откуда извлечен Excel</param>
        /// <returns>Возвращает массив картежей (номер сметы, стоимость трудоемкости работ)</returns>
        public List<(string estimateNumber, decimal cost)>? GetEstimatesWithDoneSmrCosts(string path, int page, string? type)
        {
            if (string.IsNullOrEmpty(path))
            {
                return Array.Empty<(string, decimal)>().ToList();
            }

            var searchingKeys = _estimateService.ReturnKeysSearch(type);
            var excel = _excelReader.GetExcelWorksheet(path, page);

            if (GetCellValue(excel, shiftRow: 0, shiftCol: 0, searchingKeys.DoneSmrCost.DocName.ToArray()) == string.Empty)
            {
                return Array.Empty<(string, decimal)>().ToList();
            }

            var shiftRow = 0;
            if (type == ConstantsApp.SXW_SINKEVICH_APP)
            {
                shiftRow += 1;
            }

            var columnCoordinate = _excelReader.FindCellByQuery(excel, searchingKeys.DoneSmrCost.ColName.ToArray())?.FirstOrDefault();
            if (columnCoordinate == null)
            {
                return Array.Empty<(string, decimal)>().ToList();
            }

            return ReturnListEstNumbersWithCosts(excel, (columnCoordinate?.Col ?? 0), searchingKeys.DoneSmrCost.RowName,
                                                    shiftRow, searchingKeys.DoneSmrCost.ExtraColName.FirstOrDefault(),
                                                    searchingKeys.DoneSmrCost.ExtraRowName, (columnCoordinate?.Row ?? 0));
        }


        private string GetCellValue(ExcelWorksheet excel, int shiftRow, int shiftCol, params string[] names)
        {
            //вернет первое значение строку, второе - столбец
            var cell = _excelReader.FindCellByQuery(excel, names);
            if (cell.Count() == 0)
                return string.Empty;
            int rowDates = cell.FirstOrDefault().Item1 + shiftRow;
            int startColumnOfDates = cell.FirstOrDefault().Item2 + shiftCol;
            return excel.Cells[rowDates, startColumnOfDates].Text?.Trim() ?? string.Empty;
        }

        private string GetValueInRow(ExcelWorksheet excel, int shiftRow, int shiftCol)
        {
            //вернет первое значение строку, второе - столбец            
            var value = excel.Cells[shiftRow, shiftCol].Text?.Trim();
            if (value.Contains(':'))
            {
                var mas = value.Split(':');
                return mas[1].Trim();
            }
            for (int i = shiftCol + 1; i <= excel.Dimension.End.Column; i++)
            {
                var valText = excel.Cells[shiftRow, i].Text?.Trim();
                if (valText != string.Empty)
                    return valText;
            }
            return string.Empty;
        }

        private decimal ReturnResultSearching(ExcelWorksheet excel, IEnumerable<(int, int)> nameColumn, List<string> rowName, string estimateNumber, int shiftRow = 0, string? extraCol = null, List<string>? extraRow = null)
        {
            var rowNameList = new List<string>();
            foreach (var item in rowName)
            {
                rowNameList.Add($"{item}{estimateNumber}");
            }

            var nameRowEstimate = _excelReader.FindCellByQuery(excel, rowNameList.ToArray());
            var row = nameRowEstimate.FirstOrDefault().Item1 + shiftRow;
            var col = nameColumn.FirstOrDefault().Item2;

            if (extraCol != null)
            {
                var indexRow = nameColumn.FirstOrDefault().Item1 + 1;
                for (int indexCol = col; indexCol <= excel.Dimension.End.Column; indexCol++)
                {
                    var text = excel.Cells[indexRow, indexCol].Value?.ToString();

                    if (!string.IsNullOrEmpty(text) && text.Equals(extraCol, StringComparison.OrdinalIgnoreCase))
                    {
                        col = indexCol;
                        break;
                    }
                    if (indexCol == excel.Dimension.End.Column)
                    {
                        indexCol = col;
                        if (indexRow != excel.Dimension.End.Row)
                        {
                            indexRow++;
                        }
                    }
                }
            }

            if (extraRow != null)
            {
                var nameRowEstimate2 = _excelReader.FindCellByQuery(excel, extraRow.ToArray());
                int row2 = row;

                foreach (var item in nameRowEstimate2)
                {
                    if (row2 < item.Item1)
                    {
                        row = item.Item1;
                        break;
                    }
                }
            }

            var result = (col == 0 || row == 0) ? 0M : excel.Cells[row, col].Value ?? 0M;

            return Convert.ToDecimal(result);
        }


        private List<(string estimateNumber, decimal cost)>? ReturnListEstNumbersWithCosts(ExcelWorksheet excel, int columnValue,
            List<string> rowName, int shiftRow = 0, string? extraCol = null, List<string>? extraRow = null, int? rowStart = null)
        {

            var listResult = new List<(string estimateNumber, decimal cost)>();
            var nameRowEstimate = _excelReader.FindCellByQuery(excel, rowName.ToArray());

            int columnCostValue = extraCol != null && nameRowEstimate?.FirstOrDefault() != null ?
                                   SearchExtraColumn(excel, columnValue, rowStart ?? 1, extraCol) : columnValue;
            int rowCostValue = extraRow != null ? SearchExtraRow(excel, rowStart ?? 1, extraRow) : 0;
            
            try
            {
                foreach (var estCoordinate in nameRowEstimate)
                {
                    var row = extraRow == null ? (estCoordinate.Row + shiftRow) : rowCostValue;
                    var columnName = estCoordinate.Col;

                    /* 
                     * Находим и конвертируем стоимости по смете
                     *  -- columnCostValue - столбец где стоимость;
                     *  -- row - строка стоимости
                     *  -- shiftRow - количество строк (для смещения), которые добаляем, если в документе стоимость указана 
                      *  на несколько или одну строку ниже, чем название сметы  
                    */
                    var costObj = (columnName == 0 || row == 0) ? 0M : excel.Cells[row, columnCostValue].Value ?? 0M;
                    var cost = Convert.ToDecimal(costObj);

                    /* 
                     * Находим название сметы, и парсим из нее номер сметы
                     *  -- columnName - столбец где название сметы;                    
                     *  -- estCoordinate.Row - строка (без смещения), где находится наименование сметы
                     */
                    var estimateName = (columnName == 0 || row == 0) ? "" : excel.Cells[estCoordinate.Row, columnName].Value ?? "";
                    var numberEstimate = _textSearcher.SearchNumberWithStart(estimateName?.ToString());

                    listResult.Add((numberEstimate, cost));
                }
                return listResult;
            }
            catch (Exception)
            {
                return Array.Empty<(string, decimal)>().ToList();
            }
        }


        /// <summary>
        /// Проверяет, есть ли на странице смета с необходимым номером
        /// </summary>
        /// <param name="excel"></param>
        /// <param name="drawingsName"></param>
        /// <param name="numberEstimate"></param>
        /// <param name="IsSplite"></param>
        /// <returns></returns>
        private bool ExistEstmtNumberIntoSheet(ExcelWorksheet excel, string drawingsName, string? numberEstimate, bool? IsSplite = null)
        {
            var coordinatesLineWithEstNumber = _excelReader.FindCellByQuery(excel, drawingsName).FirstOrDefault();
            var lineWithEstNumber = (coordinatesLineWithEstNumber.Item1 == 0 || coordinatesLineWithEstNumber.Item2 == 0) ? string.Empty :
                            (excel.Cells[coordinatesLineWithEstNumber.Item1, coordinatesLineWithEstNumber.Item2].Value?.ToString() ?? string.Empty);

            if (string.IsNullOrEmpty(lineWithEstNumber) || !lineWithEstNumber.Contains(numberEstimate))
            {
                return true;
            }
            return false;
        }

        private int SearchExtraColumn(ExcelWorksheet excel, int startColIndex, int startRowIndex, string extraCol)
        {
            int result = startColIndex;
            if (startRowIndex > 0 && startColIndex > 0)
            {
                var indexRow = startRowIndex + 1;
                for (int indexCol = result; indexCol <= excel.Dimension.End.Column; indexCol++)
                {
                    var text = excel.Cells[indexRow, indexCol].Value?.ToString();

                    if (!string.IsNullOrEmpty(text) && text.Equals(extraCol, StringComparison.OrdinalIgnoreCase))
                    {
                        result = indexCol;
                        break;
                    }
                    if (indexCol == excel.Dimension.End.Column)
                    {
                        indexCol = result;
                        if (indexRow != excel.Dimension.End.Row)
                        {
                            indexRow++;
                        }
                    }
                }
            }
            return result;
        }

        private int SearchExtraRow(ExcelWorksheet excel, int startRowIndex, List<string>? extraRow)
        {
            /* 
             Находим первый индекс строки, который больше стартового значения
            (стартовое значение -- расположение название сметы, 
            следующее должно содержать строку с итоговой стоимостью)
             */
            int result = startRowIndex;
            if (extraRow != null)
            {
                var nameRowEstimate2 = _excelReader.FindCellByQuery(excel, extraRow.ToArray());

                foreach (var item in nameRowEstimate2)
                {
                    if (result < item.Item1)
                    {
                        result = item.Item1;
                        break;
                    }
                }
            }
            return result;
        }
    }
}
