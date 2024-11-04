using OfficeOpenXml.Style;
using BusinessLayer.Helpers;
using OfficeOpenXml;
using System.Drawing;
using BusinessLayer.Interfaces.CommonInterfaces;

namespace BusinessLayer.ServicesCOM
{
    public class ExcelWriter : IExcelWriter
    {
        private ExcelPackage _excelPackage;
        private ExcelWorkbook _excelWorkbook;
        private ExcelWorksheet _excelWorksheet;

        public ExcelWorksheet Settup(string path, params string[] nameSheets)
        {
            if (File.Exists(path))
                File.Delete(path);

            _excelPackage = new ExcelPackage(new FileInfo(path));
            _excelWorkbook = _excelPackage.Workbook;
            if (nameSheets.Length > 0)
            {
                foreach (var sheet in nameSheets)
                {
                    _excelWorksheet = _excelWorkbook.Worksheets.Add(sheet);
                }
            }
            else
            {
                _excelWorksheet = _excelWorkbook.Worksheets.Add(ConstantsApp.WORKSHEET_NAME_DEFAULT);
            }

            _excelWorksheet.TabColor = Color.Black;
            _excelWorksheet.DefaultRowHeight = 14;

            return _excelWorksheet;
        }

        public bool WriteLine(ExcelWorksheet sheet, int rowLine, string path, bool isHeader = false, params RowItem[] values)
        {
            if (rowLine > 0 && values.Length != 0)
            {
                if (isHeader)
                {
                    sheet.Row(rowLine).Height = 20;
                    sheet.Row(rowLine).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Row(rowLine).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    sheet.Row(rowLine).Style.Font.Bold = true;
                }
                
                foreach (var value in values)
                {
                    sheet.Cells[rowLine, value.Col].Style.Font.Size = value.FontSize;
                    sheet.Cells[rowLine, value.Col].Style.Font.Color.SetColor(value.FontColor);
                    if (!value.BgColor.IsEmpty)
                    {
                        sheet.Cells[rowLine, value.Col].Style.Fill.SetBackground(value.BgColor);
                    }
                    
                    sheet.Cells[rowLine, value.Col].Value = value.Value;
                    sheet.Column(value.Col).AutoFit();
                }

                try
                {                    
                     File.WriteAllBytes(path,  _excelPackage.GetAsByteArray());
                    _excelPackage.SaveAsync();
                    return true;
                }
                catch (Exception)
                {
                    CloseExcel();
                    return false;
                }
            }
            return false;
        }

        public bool CreateExcel(ExcelPackage excel, string path)
        {
            try
            {
                File.WriteAllBytes(path, excel.GetAsByteArray());
                excel.Dispose();
                Console.ReadKey();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void CloseExcel()
        {
            _excelWorksheet.Dispose();
            _excelWorkbook.Dispose();
            _excelPackage.Dispose();
        }
    }

    public class RowItem
    {
        public string Value { get; set; }
        public int Col { get; set; } = 1;
        public int FontSize { get; set; } = 12;
        public Color FontColor { get; set; } = Color.Black;
        public Color BgColor { get; set; }
    }
}
