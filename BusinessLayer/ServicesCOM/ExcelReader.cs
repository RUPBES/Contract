using BusinessLayer.Interfaces.CommonInterfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.ServicesCOM
{
    internal class ExcelReader : IExcelReader
    {
        private ExcelPackage OpenFileExcel(string path)
        {
            try
            {
                var point = path.LastIndexOf(".");
                var type = path.Substring(point + 1);
                if (type.ToLower() == "xls")
                {
                    throw new Exception();
                }
                FileInfo file = new FileInfo(path);
                ExcelPackage package = new ExcelPackage(file);
                return package;
            }
            catch { return new ExcelPackage(); }
        }
        public ExcelWorksheet GetExcelWorksheet(string path, int page)
        {
            ExcelPackage package = OpenFileExcel(path);
            ExcelWorksheet sheet = package.Workbook.Worksheets[page];
            return sheet;
        }
        public Boolean FindByWords(string target, params string[] query)
        {
            try
            {
                target = target.ToLower();
                foreach (var item in query)
                {
                    var fl = true;
                    var mas = item.Split(' ');
                    if (mas.Length == 0) { fl = false; break; }
                    foreach (var word in mas)
                    {
                        var minWord = word.ToLower();
                        if (!target.Contains(minWord, StringComparison.OrdinalIgnoreCase))
                        { fl = false; break; }
                    }
                    if (fl)
                    { return true; }
                }
                return false;
            }
            catch { return false; }
        }

        public IEnumerable<(int, int)> FindCellByQuery(ExcelWorksheet worksheet, params string[] query)
        {
            var start = worksheet.Dimension.Start;
            var end = worksheet.Dimension.End;
            var listAnswer = new List<(int row, int col)>();
            for (int row = start.Row; row <= end.Row; row++)
                for (int col = start.Column; col <= end.Column; col++)
                {
                    string cellValue = worksheet.Cells[row, col].Text.ToString().Trim();
                    if (FindByWords(cellValue, query))
                    {
                        (int row, int col) answer;
                        answer.row = row;
                        answer.col = col;
                        listAnswer.Add(answer);
                    }
                }
            return listAnswer;
        }

        public IEnumerable<ExcelWorksheet> GetListOfBook(string path)
        {
            ExcelPackage package = OpenFileExcel(path);
            if (package.Workbook.Worksheets.Count > 0)
            {
                var list = new List<ExcelWorksheet>();
                foreach (var item in package.Workbook.Worksheets)
                {
                    list.Add(item);
                }
                return list;
            }
            else
                return Enumerable.Empty<ExcelWorksheet>();
        }

        public double GetValueDouble(ExcelWorksheet worksheet, int row, int col)
        {
            try
            {
                var ob = worksheet.Cells[row, col].Value?.ToString().Trim();
                double answer = 0;
                var isDouble = double.TryParse(ob, out answer);
                if (!isDouble) return 0;
                else return answer;
            }
            catch { return 0; }
        }

        public string GetValueString(ExcelWorksheet worksheet, int row, int col)
        {
            try
            {
                var ob = worksheet.Cells[row, col].Value?.ToString().Trim();
                if (ob == null) return "";
                else return ob;
            }
            catch { return ""; }
        }
    }
}
