using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces.COMServices;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using BusinessLayer.Models.Settings;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Reflection;

namespace BusinessLayer.ServicesCOM;

internal class ReportExcelService : IReportExcelService
{
    private readonly IConverterService _converter;
    private readonly IContractUoW _contextDb;
    private readonly IExcelWriter _excelWriter;
    private readonly IHostingEnvironment _host;
    private readonly IScopeWorkService _scopeWork;
    private readonly IFormService _formService;
    private readonly IVContractService _vContractService;
    private readonly IContractService _contractService;
    private readonly IAmendmentService _amendmentService;
    private readonly ISelectionProcedureService _selection;
    private readonly IEmployeeService _employee;
    private readonly IOrganizationService _organization;
    private readonly IMapper _mapper;
    private readonly IContractArchiveUoW _databaseArch;
    private readonly IPaymentService _paymentService;
    private readonly IHttpContextUserProvider _httpHelper;

    public ReportExcelService(IConverterService converter, IContractUoW contextDb, IExcelWriter excelWriter, IHostingEnvironment hosting, IScopeWorkService scopeWork,
        IFormService formService, IContractService contractService, IAmendmentService amendmentService, IVContractService vContractService,
        IMapper mapper, ISelectionProcedureService selection, IEmployeeService employee, IOrganizationService organization, IContractArchiveUoW databaseArch,
        IPaymentService paymentService, IHttpContextUserProvider httpHelper)
    {
        _converter = converter;
        _contextDb = contextDb;
        _excelWriter = excelWriter;
        _host = hosting;
        _scopeWork = scopeWork;
        _formService = formService;
        _contractService = contractService;
        _amendmentService = amendmentService;
        _vContractService = vContractService;
        _mapper = mapper;
        _selection = selection;
        _employee = employee;
        _organization = organization;
        _databaseArch = databaseArch;
        _paymentService = paymentService;
        _httpHelper = httpHelper;
    }


    public async Task<string> ExportContracts(string organization, List<string> columnName, bool? useArchiveData)
    {
        if (!string.IsNullOrEmpty(organization) && organization != "all")
        {
            var contracts = (useArchiveData == true) ?
                _databaseArch.vContracts.Find(x => x.Owner == organization).Select(c => GetProperties(c, columnName)) :
                _contextDb.vContracts.Find(x => x.Owner == organization).Select(c => GetProperties(c, columnName));
            return await ExportContractsExcelAsync(contracts);
        }
        else
        {
            var contracts = (useArchiveData == true) ?
                _databaseArch.vContracts.GetAll().Select(c => GetProperties(c, columnName)) :
                _contextDb.vContracts.GetAll().Select(c => GetProperties(c, columnName));

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
                foreach (var colName in contracts.FirstOrDefault()?.Select(x => x.Key) ?? Array.Empty<string>())
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
                _excelWriter.WriteLine(sheet, startRow++, path, true, null, width: null, isTextWrap: null, ExcelHorizontalAlignment.Left, rowItems.ToArray());

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
                    _excelWriter.WriteLine(sheet, startRow, path, false, null, width: 25, isTextWrap: true, align: null, rowItems2.ToArray());
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
            var contractProp = _contextDb.Contracts
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
                _excelWriter.WriteLine(sheet, startRow++, path, true, null, width: null, isTextWrap: null, ExcelHorizontalAlignment.Left, headerItems.ToArray());

                // Set body
                int nextRow = FillPeriodsTable(sheet, scopes, startRow, path, dates);
                FillPeriodsTable(sheet, forms, nextRow, path, dates);

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

    public async Task<string> ExportContractDetails(int contractId)
    {
        if (contractId < 1)
        {
            return await Task.Run(() => string.Empty);
        }
        return await ExportContractDetailsExcelAsync(contractId);
    }

    private async Task<string> ExportContractDetailsExcelAsync(int contractId)
    {
        if (!Directory.Exists(_host.WebRootPath + "\\Temp"))
        {
            Directory.CreateDirectory(_host.WebRootPath + "\\Temp");
        }

        string path = _host.WebRootPath + "\\Temp" + "\\Детали_договора.xlsx";

        await Task.Run(() =>
        {
            var sheet = _excelWriter.Settup(path, "Детальная информация по договору");
            int startCol = 1;
            var settings = new ExcelSettings
            {
                CellHeight = 55,
                RowLine = 2,
                isBold = true,
                isTextWrap = true,
                FontColor = Constants.COLOR_WHITE,
                BgColor = Constants.COLOR_DARK_BLUE,
                horizAligment = ExcelHorizontalAlignment.Center,
                vertAligment = ExcelVerticalAlignment.Center,
            };

            try
            {
                var contract = _vContractService.Find(x => x.Id == contractId).FirstOrDefault();

                ///ЗАПОЛНЯЕМ ШАПКУ ТАБЛИЦЫ
                List<RowItem> rowItems = new List<RowItem>
                {
                    new RowItem { Value = "Процедура выбора", Col = startCol++ },
                    new RowItem {Value = "Подписант договора, лицо ответственное за ведение договора", Col = startCol ++},
                    new RowItem { Value = "Номер и дата заключения договора", Col = startCol++ },
                    new RowItem {Value = "Наименование объекта", Col = startCol ++},
                    new RowItem {Value = "Источник финан-сирования", Col = startCol ++},
                    new RowItem {Value = "Заказчик", Col = startCol ++},
                    new RowItem {Value = "Генподрядчик", Col = startCol ++},
                    new RowItem {Value = "Субподрядчик", Col = startCol ++},
                    new RowItem {Value = "Cроки", Col = startCol ++},
                    new RowItem {Value = "Условия оплаты, без авансов тек. %\r / цел.%,\n отсрочка/в течение", Col = startCol ++},
                    new RowItem {Value = "Виды работ по договору", Col = startCol ++},
                    new RowItem {Value = "Контрактная цена, с НДС", Col = startCol ++},
                    new RowItem {Value = "Валюта", Col = startCol ++},
                };

                _excelWriter.WriteLine(sheet, path, settings, rowItems.ToArray());

                startCol = 1;
                settings.RowLine++;
                settings.CellHeight = null;
                settings.FontColor = Constants.COLOR_BLACK;
                settings.BgColor = Constants.COLOR_GRAY_LIGHT;
                settings.horizAligment = ExcelHorizontalAlignment.Left;
                settings.vertAligment = ExcelVerticalAlignment.Top;

                FillDetailsTable(sheet, path, settings, contract, isGenContract: true);

                var subSubObject = _contractService.Find(x => x.MultipleContractId == contractId & x.IsOneOfMultiple == true);

                if (subSubObject.Any())
                {
                    settings.RowLine++;
                    settings.FontColor = Constants.COLOR_BLACK;
                    settings.BgColor = Color.Empty;
                    settings.FontSize = Constants.FONT_SIZE_12;
                    settings.isMergeCell = true;
                    settings.mergeStartCell = startCol;
                    settings.mergeCountCell = 13;
                    settings.horizAligment = ExcelHorizontalAlignment.Center;
                    settings.vertAligment = ExcelVerticalAlignment.Center;

                    _excelWriter.WriteLine(sheet, path, settings, new RowItem { Value = "Подобъекты", Col = 1 });

                    int countSubObj = 1;

                    foreach (var sub in subSubObject)
                    {
                        settings.RowLine++;
                        settings.mergeStartCell = startCol;
                        settings.mergeCountCell = 3;
                        settings.BgColor = Constants.COLOR_ORANGE_SUBOGJ;
                        settings.horizAligment = ExcelHorizontalAlignment.Left;
                        settings.vertAligment = ExcelVerticalAlignment.Top;

                        var subObj = _mapper.Map<VContractDTO>(sub);
                        subObj.ProcedureName = $"ПОДОБЪЕКТ №{countSubObj}";

                        FillDetailsTable(sheet, path, settings, subObj, isGenContract: false);

                        startCol = 1;
                        var subSubObject2 = _contractService.Find(x => x.AgreementContractId == sub.Id || x.SubContractId == sub.Id);

                        if (subSubObject2.Any())
                        {
                            settings.RowLine++;
                            settings.FontColor = Constants.COLOR_BLACK;
                            settings.BgColor = Color.Empty;
                            settings.FontSize = Constants.FONT_SIZE_12;
                            settings.mergeStartCell = startCol;
                            settings.mergeCountCell = 13;
                            settings.horizAligment = ExcelHorizontalAlignment.Center;
                            settings.vertAligment = ExcelVerticalAlignment.Center;

                            _excelWriter.WriteLine(sheet, path, settings, new RowItem { Value = $"Субподрядные договора подобъекта №{countSubObj}", Col = 1 });
                        }

                        settings.isMergeCell = null;

                        foreach (var subagr in subSubObject2)
                        {
                            settings.RowLine++;
                            settings.horizAligment = ExcelHorizontalAlignment.Left;
                            settings.vertAligment = ExcelVerticalAlignment.Top;

                            var subagr2 = _mapper.Map<VContractDTO>(subagr);
                            subagr2.ProcedureName = _selection?.Find(x => x.ContractId == subagr.Id)?.FirstOrDefault()?.TypeProcedure;
                            subagr2.ResponsibleEmp = _employee.FindByContractEmployee(x => x.ContractId == subagr.Id && x.IsResponsible == true)?.FullName;
                            subagr2.SignatoryEmp = _employee.FindByContractEmployee(x => x.ContractId == subagr.Id && x.IsSignatory == true)?.FullName;
                            subagr2.GenContractor = _organization.FindByContractOrganization(x => x.ContractId == subagr.Id)?.Name;

                            FillDetailsTable(sheet, path, settings, subagr2, isGenContract: false);
                        }
                        countSubObj++;
                    }
                }

                var subContracts = _contractService.Find(x => x.AgreementContractId == contract.Id || x.SubContractId == contract.Id);

                if (subContracts.Any())
                {
                    settings.RowLine++;
                    settings.BgColor = Constants.COLOR_EMPTY;
                    settings.isMergeCell = true;
                    settings.mergeStartCell = startCol;
                    settings.mergeCountCell = 13;
                    settings.horizAligment = ExcelHorizontalAlignment.Center;
                    settings.vertAligment = ExcelVerticalAlignment.Center;

                    _excelWriter.WriteLine(sheet, path, settings, new RowItem { Value = "Субподрядные договора генподряда", Col = 1 });

                    settings.isMergeCell = null;

                    foreach (var subagr in subContracts)
                    {
                        settings.RowLine++;
                        settings.horizAligment = ExcelHorizontalAlignment.Left;
                        settings.vertAligment = ExcelVerticalAlignment.Top;
                        settings.BgColor = subagr.IsSubContract == true ?
                                        Constants.COLOR_OLIVEDROB_LIGHT :
                                        (subagr.IsAgreementContract == true ?
                                                Constants.COLOR_SKYBLUE_LIGHT :
                                                Constants.COLOR_EMPTY);

                        var subagr2 = _mapper.Map<VContractDTO>(subagr);
                        subagr2.ProcedureName = _selection?.Find(x => x.ContractId == subagr.Id)?.FirstOrDefault()?.TypeProcedure;
                        subagr2.ResponsibleEmp = _employee.FindByContractEmployee(x => x.ContractId == subagr.Id && x.IsResponsible == true)?.FullName;
                        subagr2.SignatoryEmp = _employee.FindByContractEmployee(x => x.ContractId == subagr.Id && x.IsSignatory == true)?.FullName;
                        subagr2.GenContractor = _organization.FindByContractOrganization(x => x.ContractId == subagr.Id)?.Name;

                        FillDetailsTable(sheet, path, settings, _mapper.Map<VContractDTO>(subagr2), isGenContract: false);
                    }
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




    public async Task<string> ExportAmountDueToExcel(string docName, FilterPayableModel filter)
    {
        if (string.IsNullOrEmpty(docName))
        {
            return await Task.Run(() => string.Empty);
        }
        return await ExportAmountDueToExcelAsync(docName, filter);
    }

    private async Task<string> ExportAmountDueToExcelAsync(string docName, FilterPayableModel filter)
    {
        if (!Directory.Exists(_host.WebRootPath + "\\Temp"))
        {
            Directory.CreateDirectory(_host.WebRootPath + "\\Temp");
        }

        string path = _host.WebRootPath + "\\Temp" + $"\\{docName}.xlsx";

        await Task.Run(() =>
        {
            var sheet = _excelWriter.Settup(path, docName);
            int startCol = 1;
            var settings = new ExcelSettings
            {
                CellHeight = 60,
                RowLine = 2,
                isBold = true,
                isTextWrap = true,
                FontColor = Constants.COLOR_WHITE,
                BgColor = Constants.COLOR_DARK_BLUE,
                horizAligment = ExcelHorizontalAlignment.Center,
                vertAligment = ExcelVerticalAlignment.Center,
            };

            try
            {
                var amountDue = _paymentService.GetPayableCash(filter, _httpHelper.GetUserOrganizationCodes().Split(','));

                ///ЗАПОЛНЯЕМ ШАПКУ ТАБЛИЦЫ
                List<RowItem> rowItems = new List<RowItem>
                {
                    new RowItem { Value = "Номер и дата договора", Col = startCol++ },
                    new RowItem {Value = "Наименование объекта", Col = startCol ++},
                    new RowItem { Value = "Заказчик", Col = startCol++ },
                    new RowItem {Value = "Генподрядчик (филиал-исполнитель по приказу)", Col = startCol ++},
                    new RowItem {Value = "Сроки выполнения работ", Col = startCol ++},
                    new RowItem {Value = "Срок ввода", Col = startCol ++},
                    new RowItem {Value = "Валюта расчетов", Col = startCol ++},
                    new RowItem {Value = "Всего по договору с НДС", Col = startCol ++},
                    new RowItem {Value = "Остаток по выполнению", Col = startCol ++},
                    new RowItem {Value = "Объем на текущий год", Col = startCol ++},
                    new RowItem {Value = "Фактическое выполнение по справке С-3а", Col = startCol ++},
                    new RowItem {Value = "Зарезервированные средства", Col = startCol ++},
                };

                _excelWriter.WriteLine(sheet, path, settings, rowItems.ToArray());

                startCol = 1;
                settings.RowLine++;
                settings.CellHeight = null;
                settings.BgColor = Color.Empty;
                settings.FontColor = Constants.COLOR_BLACK;
                settings.horizAligment = ExcelHorizontalAlignment.Left;
                settings.vertAligment = ExcelVerticalAlignment.Top;
                settings.FontSize = Constants.FONT_SIZE_12;


                if (amountDue.Any())
                {
                    foreach (var amount in amountDue)
                    {
                        startCol = 1;
                        List<RowItem> itemsAmount = new List<RowItem>
                        {
                            new RowItem { Value = (amount?.Number) != null? $"{amount?.Number} от {amount?.Date?.ToShortDateString()}" : amount?.Date?.ToShortDateString(), Col = startCol++ },
                            
                            new RowItem {Value = amount?.NameObject, Col = startCol ++},
                            new RowItem { Value = amount?.Client, Col = startCol++ },
                            new RowItem {Value = amount?.GenContractor, Col = startCol ++},
                            
                            new RowItem {Value = amount?.DateBeginWork?.ToShortDateString() != null?
                                        $"{amount?.DateBeginWork?.ToShortDateString()} - {amount?.DateEndWork?.ToShortDateString()}" :
                                        amount?.DateEndWork?.ToShortDateString(), Col = startCol ++},
                            
                            new RowItem {Value = amount?.EnteringTerm?.ToShortDateString(), Col = startCol ++},
                            new RowItem {Value = amount?.Сurrency, Col = startCol ++},
                            new RowItem {Value =amount?.ContractPrice?.ToString("N2"), Col = startCol ++},
                            new RowItem {Value = (amount?.ContractPrice - amount?.FactSum)?.ToString("N2") , Col = startCol ++},
                            new RowItem {Value = amount?.ThisYearSum?.ToString("N2"), Col = startCol ++},
                            new RowItem {Value = amount?.FactSum?.ToString("N2"), Col = startCol ++},
                            new RowItem {Value = amount?.ReserveSum?.ToString("N2"), Col = startCol ++},
                        };

                        settings.RowLine++;
                        _excelWriter.WriteLine(sheet, path, settings, itemsAmount.ToArray());
                    }
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

    private int FillPeriodsTable(ExcelWorksheet sheet, ScopeWorkReportModel groupe, int startRow, string path, List<DateTime> dates)
    {
        foreach (var itemGroup in groupe.Scopes)
        {
            int startCol = 1;
            _excelWriter.WriteLine(sheet, startRow++, path, false, null, width: null, isTextWrap: null, ExcelHorizontalAlignment.Left,
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
                _excelWriter.WriteLine(sheet, startRow++, path, false, null, width: null, isTextWrap: false, align: null, rowItems.ToArray());
                startCol = 1;
            }
        }
        return startRow;
    }

    private void FillDetailsTable(ExcelWorksheet sheet, string path, ExcelSettings settings, VContractDTO? contract, bool isGenContract)
    {
        var amendment = _amendmentService
            .Find(x => x.ContractId == contract.Id)
            .OrderBy(x => x.Date)
            .Select(x => new AmendmentDTO
            {
                ContractPrice = x.ContractPrice,
                DateBeginWork = x.DateBeginWork,
                DateEndWork = x.DateEndWork,
                DateEntryObject = x.DateEntryObject
            })
            .LastOrDefault();

        if (amendment != null)
        {
            contract.ContractPrice = amendment.ContractPrice ?? contract.ContractPrice;
            contract.DateBeginWork = amendment.DateBeginWork ?? contract.DateBeginWork;
            contract.DateEndWork = amendment.DateEndWork ?? contract.DateEndWork;
            contract.EnteringTerm = amendment.DateEntryObject ?? contract.EnteringTerm;
        }

        int startCol = 1;
        string number = string.Empty;

        if (contract?.Date?.ToShortDateString() != null && contract.Number != null)
        {
            number = $"{contract.Number}\n от {contract?.Date?.ToShortDateString()}";
        }
        else if (contract?.Date?.ToShortDateString() == null && contract.Number != null)
        {
            number = $"{contract.Number}";
        }
        else if (contract?.Date?.ToShortDateString() != null && contract.Number == null)
        {
            number = $"{contract?.Date?.ToShortDateString()}";
        }


        string employees = string.Empty;

        if (contract?.SignatoryEmp != null && contract?.ResponsibleEmp != null)
        {
            employees = $"Подписант - {contract?.SignatoryEmp},\n Лицо, ответственное за ведение договора - {contract?.ResponsibleEmp}";
        }
        else if (contract?.SignatoryEmp == null && contract?.ResponsibleEmp != null)
        {
            employees = $"Лицо, ответственное за ведение договора - {contract?.ResponsibleEmp}";
        }
        else if (contract?.SignatoryEmp != null && contract?.ResponsibleEmp == null)
        {
            employees = $"Подписант - {contract?.SignatoryEmp}";
        }

        List<RowItem> rowItems = new List<RowItem>
        {
            new RowItem { Value = contract?.ProcedureName, Col = startCol++, ColWidth= 18 },
            new RowItem { Value = employees, Col = startCol++, ColWidth= 30 },
            new RowItem { Value = number, Col = startCol++,ColWidth= 15 },
            new RowItem { Value = contract?.NameObject, Col = startCol++,ColWidth= 30,  },
            new RowItem { Value = contract.FundingSource, Col = startCol ++, ColWidth = 20},
            new RowItem { Value = contract?.Client, Col = startCol ++, ColWidth = 27},
            new RowItem { Value = isGenContract? contract?.GenContractor : null, Col = startCol ++, ColWidth = 27},
            new RowItem { Value = isGenContract? null : contract?.GenContractor, Col = startCol ++, ColWidth = 27},
            new RowItem
            {
                Value = $"выполнения работ: \n {contract?.DateBeginWork?.ToShortDateString()} - {contract?.DateEndWork?.ToShortDateString()} " +
                $"\n ввода: \n  {contract?.EnteringTerm?.ToShortDateString()} \nдействия договора: \n  { contract?.ContractTerm?.ToShortDateString() }",
                Col = startCol++,
                ColWidth= 15
            },
            new RowItem { Value = contract.PaymentСonditionsAvans + "\n " + contract.PaymentСonditionsRaschet, Col = startCol++, ColWidth= 30 },
            new RowItem { Value = contract.WorkType, Col = startCol++,ColWidth= 20 },
            new RowItem { Value = contract.ContractPrice?.ToString("N2"), Col = startCol++, ColWidth= 18 },
            new RowItem { Value = contract?.Сurrency, Col = startCol++, ColWidth= 9 },
        };

        _excelWriter.WriteLine(sheet, path, settings, rowItems.ToArray());
    }
}