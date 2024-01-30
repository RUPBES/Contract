using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Models;
using OfficeOpenXml;

namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface IParsService
    {
        IEnumerable<ExcelWorksheet> getListOfBook(string path);
        ExcelWorksheet GetExcelWorksheet(string path, int page);
        double GetValueDouble(ExcelWorksheet worksheet, int row, int col);
        string GetValueString(ExcelWorksheet worksheet, int row, int col);
        IEnumerable<(int, int)> FindCellByQuery(ExcelWorksheet worksheet, params string[] query);
        Boolean FindByWords(string target, params string[] query);

        FormDTO Pars_C3A(string path, int page);
    }
}
