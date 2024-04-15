using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface IExcelReader
    {
        IEnumerable<ExcelWorksheet> GetListOfBook(string path);
        ExcelWorksheet GetExcelWorksheet(string path, int page);
        double GetValueDouble(ExcelWorksheet worksheet, int row, int col);
        string GetValueString(ExcelWorksheet worksheet, int row, int col);
        IEnumerable<(int, int)> FindCellByQuery(ExcelWorksheet worksheet, params string[] query);
        Boolean FindByWords(string target, params string[] query);
    }
}
