using BusinessLayer.Interfaces.COMServices;
using BusinessLayer.Models.Settings;
using Microsoft.AspNetCore.Mvc;

namespace MvcLayer.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportExcelService _reportExcel;

        public ReportController(IReportExcelService reportExcel)
        {
            _reportExcel = reportExcel;
        }

        [Route("/Report/Print/Contracts")]
        public IActionResult SelectContractProperties()
        {
            var listProps = new Dictionary<string, string>();

            listProps.Add("Number", "Номер договора");
            listProps.Add("Date", "Дата заключения договора");
            listProps.Add("ContractTerm", "Срок действия договора");
            listProps.Add("DateBeginWork", "Начало работ");
            listProps.Add("DateEndWork", "Окончание работ");
            listProps.Add("NameObject", "Наименование объекта");
            listProps.Add("Client", "Заказчик");
            listProps.Add("GenContractor", "Генподрядчик");
            listProps.Add("ResponsibleForWork", "Ответственный за производство работ");
            listProps.Add("EnteringTerm", "Срок ввода");
            listProps.Add("PaymentСonditionsAvans", "Условия авансирования");
            listProps.Add("PaymentСonditionsRaschet", "Расчет за выполненные работы");
            listProps.Add("Сurrency", "Валюта");
            listProps.Add("WorkType", "Виды работ по договору");
            listProps.Add("ContractPrice", "Всего по договору с НДС");
            listProps.Add("PreYearSum", "Выполнено на 01.01 тек. года");
            listProps.Add("RemainingSum", "Остаток");
            listProps.Add("ThisYearSum", "Объем на текущий год");

            return View(listProps);
        }

        [HttpPost]
        public async Task<IActionResult> PrintContracts(string organization, bool useArchiveData, List<string> props)
        {
            var path = Task.Run(() =>
            {
                return _reportExcel.ExportContracts(organization, props, useArchiveData);
            });

            var fileStream = new FileStream(path.Result, FileMode.OpenOrCreate, FileAccess.Read);

            // Устанавливаем заголовок Content-Length
            var fileLength = new FileInfo(path.Result).Length;
            Response.Headers.Add("Content-Length", fileLength.ToString());

            return await Task.FromResult<IActionResult>(File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Договоры.xlsx"));
        }


        [Route("/Report/Print/Contracts/Details")]
        public async Task<IActionResult> PrintContractDetails(int contractId)
        {
            var path = Task.Run(async () =>
            {
                return await _reportExcel.ExportContractDetails(contractId);
            });

            var fileStream = new FileStream(path.Result, FileMode.OpenOrCreate, FileAccess.Read);

            // Устанавливаем заголовок Content-Length
            var fileLength = new FileInfo(path.Result).Length;
            Response.Headers.Add("Content-Length", fileLength.ToString());

            return await Task.FromResult<IActionResult>(File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Детальная информация.xlsx"));
        }

        [Route("/Report/Print/Scopes")]
        public async Task<IActionResult> PrintScope(int contractId, string? numberContr)
        {
            //var fileName = $"Объем работ. Договор {"№ " + numberContr}.xlsx";
            var path = Task.Run(() =>
            {
                return _reportExcel.ExportScopeWorkToExcel("Объем работ.xlsx", contractId);
            });
            if (path.Result == string.Empty)
            {
                return await Task.FromResult<IActionResult>(View("Index","Contracts"));
            }
            var fileStream = new FileStream(path.Result, FileMode.Open, FileAccess.Read);

            //// Устанавливаем заголовок Content-Length
            var fileLength = new FileInfo(path.Result).Length;
            Response.Headers.Add("Content-Length", fileLength.ToString());
            return await Task.FromResult<IActionResult>(File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Объем работ.xlsx"));
        }

        [Route("/Report/Print/AmountDue")]
        public IActionResult FilterPayableCash()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PrintPayableCash(FilterPayableModel filter)
        {
            var fileName = $"Денежные средства, подлежащие к оплате";
            var path = Task.Run(() =>
            {
                return _reportExcel.ExportAmountDueToExcel(fileName, filter);
            });
            if (path.Result == string.Empty)
            {
                return await Task.FromResult<IActionResult>(View(nameof(FilterPayableCash)));
            }
            var fileStream = new FileStream(path.Result, FileMode.Open, FileAccess.Read);

            //// Устанавливаем заголовок Content-Length
            var fileLength = new FileInfo(path.Result).Length;
            Response.Headers.Add("Content-Length", fileLength.ToString());
            return await Task.FromResult<IActionResult>(File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx"));
        }
    }
}