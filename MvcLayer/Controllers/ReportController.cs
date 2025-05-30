using BusinessLayer.Interfaces.CommonInterfaces;
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
        public async Task<IActionResult> PrintContracts(string organization, List<string> props)
        {
            var path = Task.Run(() =>
            {
                return _reportExcel.ExportContracts(organization, props);
            });
            var fileStream = new FileStream(path.Result, FileMode.Open, FileAccess.Read);

            // Устанавливаем заголовок Content-Length
            var fileLength = new FileInfo(path.Result).Length;
            Response.Headers.Add("Content-Length", fileLength.ToString());

            return await Task.FromResult<IActionResult>(File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Договора.xlsx"));
        }

        public async Task<IActionResult> PrintScope(int contractId, string? numberContr)
        {
            var fileName = $"Объем работ. Договор {"№ " + numberContr}.xlsx";
            var path = Task.Run(() =>
            {
                return _reportExcel.ExportScopeWorkToExcel(fileName, contractId);
            });
            var fileStream = new FileStream(path.Result, FileMode.Open, FileAccess.Read);

            //// Устанавливаем заголовок Content-Length
            var fileLength = new FileInfo(path.Result).Length;
            Response.Headers.Add("Content-Length", fileLength.ToString());
            return await Task.FromResult<IActionResult>(File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName));
        }
    }
}
