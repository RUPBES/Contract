using BusinessLayer.Models;

namespace BusinessLayer.Interfaces.CommonInterfaces;

public interface IReportExcelService
{
    Task<string> ExportContracts(string organization, List<string> columnName);
    //string ExportScopeWorkToExcel(string fileName, int contractId, string? sheetName = "Объем работ");
    Task<string> ExportScopeWorkToExcel(string fileName, int contractId, string? sheetName = "Объем работ");
}
