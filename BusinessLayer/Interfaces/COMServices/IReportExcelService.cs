using BusinessLayer.Models;
using BusinessLayer.Models.Settings;

namespace BusinessLayer.Interfaces.COMServices;

public interface IReportExcelService
{
    Task<string> ExportContracts(string organization, List<string> columnName, bool? useArchiveData = null);
    //string ExportScopeWorkToExcel(string fileName, int contractId, string? sheetName = "Объем работ");
    Task<string> ExportScopeWorkToExcel(string fileName, int contractId, string? sheetName = "Объем работ");

    Task<string> ExportContractDetails(int contractId);

    Task<string> ExportAmountDueToExcel(string docName, FilterPayableModel filter);
}
