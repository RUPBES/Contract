using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.ServicesCOM
{
    internal class ParsService : IParsService
    {
        public IEnumerable<(int, int)> FindCellByQuery(ExcelWorksheet worksheet, string query)
        {
            var start = worksheet.Dimension.Start;
            var end = worksheet.Dimension.End;
            var listAnswer = new List<(int row, int col)>();
            for (int row = start.Row; row <= end.Row; row++)
                for (int col = start.Column; col <= end.Column; col++)
                {
                    string cellValue = worksheet.Cells[row, col].Text.ToString().Trim();
                    if (cellValue.Contains(query, StringComparison.OrdinalIgnoreCase))
                    {
                        (int row, int col) answer;
                        answer.row = row;
                        answer.col = col;
                        listAnswer.Add(answer);
                    }
                }
            return listAnswer;
        }

        public ExcelWorksheet GetExcelWorksheet(string path, int page)
        {

            FileInfo file = new FileInfo(path);
            ExcelPackage package = new ExcelPackage(file);
            ExcelWorksheet sheet = package.Workbook.Worksheets[page];
            return sheet;
        }

        public IEnumerable<string> getListOfBook(string path)
        {
            FileInfo file = new FileInfo(path);
            ExcelPackage package = new ExcelPackage(file);
            var list = new List<string>();
            foreach (var item in package.Workbook.Worksheets)
            {
                list.Add(item.Name);
            }
            return list;
        }

        public double GetValueDouble(ExcelWorksheet worksheet, int row, int col)
        {
            var ob = worksheet.Cells[row, col].Value?.ToString().Trim();
            double answer = 0;
            var isDouble = double.TryParse(ob, out answer);
            if (!isDouble) return 0;
            else return answer;
        }

        public string GetValueString(ExcelWorksheet worksheet, int row, int col)
        {
            var ob = worksheet.Cells[row, col].Value?.ToString().Trim();
            if (ob == null) return "";
            else return ob;
        }

        public FormDTO Pars_C3A(string path, int page)
        {
            var excel = GetExcelWorksheet(path, page);
            var listString = new List<(string, int)>();
            var query = new List<string>
            {
                "стоимость выполненных строительно-монтажных работ",
                "стоимость пусконаладочных работ",
                "стоимость дополнительных работ",                
                "Стоимость оборудования, поставка которого",
                "зачет целевого аванса",
                "зачет текущего аванса",
                "материалы",
                "возмещение стоимости",
                "другие"
            };

            var c3A = new FormDTO();

            var col = FindCellByQuery(excel, "за отчетный период").FirstOrDefault();
            if (col.Item2 == null) return null;

            for (var i = 0; i < query.Count; i++)
            {
                var find = FindCellByQuery(excel, query[i]);
                foreach(var item in find)
                {
                    (string, int) ob;
                    ob.Item1 = excel.Cells[item.Item1, item.Item2].Value.ToString();
                    ob.Item2 = item.Item1;
                    listString.Add(ob);
                }
            }
            if (listString.Where(x => x.Item1.Contains("стоимость выполненных строительно-монтажных работ")).FirstOrDefault().Item1 != null)
            { 
                var additionalWork = (decimal)GetValueDouble(excel, 
                    listString.Where(x => x.Item1.Contains("стоимость дополнительных работ")).First().Item2, col.Item2);
                if (additionalWork == 0)                 
                c3A.SmrCost = (decimal)GetValueDouble(excel,
                    listString.Where(x => x.Item1.Contains("стоимость выполненных строительно-монтажных работ")).First().Item2, col.Item2);
                else
                {
                    c3A.AdditionalCost = additionalWork;
                    c3A.SmrCost = (decimal)GetValueDouble(excel, 
                        listString.Where(x => x.Item1.Contains("стоимость выполненных строительно-монтажных работ")).First().Item2, col.Item2)-additionalWork;
                }
            }
            if (listString.Where(x => x.Item1.Contains("стоимость пусконаладочных работ")).FirstOrDefault().Item1 != null)
            {
                var additionalWork = (decimal)GetValueDouble(excel,
                    listString.Where(x => x.Item1.Contains("стоимость дополнительных работ")).First().Item2, col.Item2);
                if (additionalWork == 0)
                    c3A.PnrCost = (decimal)GetValueDouble(excel,
                        listString.Where(x => x.Item1.Contains("стоимость пусконаладочных работ")).First().Item2, col.Item2);
                else
                {
                    c3A.AdditionalCost = additionalWork;
                    c3A.PnrCost = (decimal)GetValueDouble(excel,
                        listString.Where(x => x.Item1.Contains("стоимость пусконаладочных работ")).First().Item2, col.Item2) - additionalWork;
                }
            }
            c3A.EquipmentCost = (decimal)GetValueDouble(excel, 
                listString.Where(x => x.Item1.Contains("Стоимость оборудования")).First().Item2, col.Item2);
            c3A.OffsetTargetPrepayment = (decimal)GetValueDouble(excel, 
                listString.Where(x => x.Item1.Contains("зачет целевого аванса")).First().Item2, col.Item2);
            c3A.OffsetCurrentPrepayment = (decimal)GetValueDouble(excel, 
                listString.Where(x => x.Item1.Contains("зачет текущего аванса")).First().Item2, col.Item2);
            c3A.MaterialCost = (decimal)GetValueDouble(excel, 
                listString.Where(x => x.Item1.Contains("материалы")).First().Item2, col.Item2);
            c3A.GenServiceCost = (decimal)GetValueDouble(excel,
                listString.Where(x => x.Item1.Contains("другие(генуслуги)")).First().Item2, col.Item2);
            c3A.OtherExpensesCost = 0;
            foreach (var item in listString.Where(x => x.Item1.Contains("другие") || x.Item1.Contains("возмещение стоимости")))
            {
                if (!item.Item1.Contains("другие(генуслуги)"))
                {
                    c3A.OtherExpensesCost += (decimal)GetValueDouble(excel,
                    item.Item2, col.Item2);
                }
            }            
            return c3A;
        }
    }
}
