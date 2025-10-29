using BusinessLayer.ServicesCOM;
using OfficeOpenXml;
using OfficeOpenXml.Core.Worksheet.Fill;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface IExcelWriter
    {
        ExcelWorksheet Settup(string path, params string[] nameSheets);
        void CloseExcel();
        bool CreateExcel(ExcelPackage excel, string path);
        bool WriteLine(ExcelWorksheet sheet, string path, ExcelSettings setting, params RowItem[] values);
        bool WriteLine(ExcelWorksheet sheet, int rowLine, string path, bool isHeader = false, double? headerHeight = null, double? width = null, bool? isTextWrap = null, ExcelHorizontalAlignment? align = null, params RowItem[] values);
    }
}
