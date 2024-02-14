using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.ServicesCOM
{
    internal class ParsService : IParsService
    {
        private ExcelPackage OpenFileExcel (string path)
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
                        word.ToLower();
                        if (!target.Contains(word, StringComparison.OrdinalIgnoreCase))
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

        public ExcelWorksheet GetExcelWorksheet(string path, int page)
        {
            ExcelPackage package = OpenFileExcel(path);
            ExcelWorksheet sheet = package.Workbook.Worksheets[page];
            return sheet;
        }

        public IEnumerable<ExcelWorksheet> getListOfBook(string path)
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

        public FormDTO Pars_C3A(string path, int page)
        {
            var excel = GetExcelWorksheet(path, page);
            var listString = new List<(string, int)>();
            var query = new List<List<string>>
            {
                new List<string>
                {
                    "стоимость выполненных строительно монтажных работ"
                },
                new List<string>
                {
                    "стоимость пусконаладочных работ"
                },
                new List<string>
                {
                    "стоимость дополнительн работ",
                    "стоимость доп работ"
                },
                new List<string>
                {
                    "сумма НДС по дополнительн работ",
                    "сумма НДС доп работ"
                },
                new List<string>
                {
                    "стоимость оборудования"
                },
                new List<string>
                {
                    "зачет целевого аванса"
                },
                new List<string>
                {
                    "зачет текущего аванса"
                },
                new List<string>
                {
                    "материалы"
                },
                new List<string>
                {
                    "возмещение стоимости",
                    "возврат стоимости"
                },
                new List<string>
                {
                    "другие"
                }
            };
            var c3A = new FormDTO
            {
                AdditionalCost = 0,
                EquipmentCost = 0,
                GenServiceCost = 0,
                MaterialCost = 0,
                OffsetCurrentPrepayment = 0,
                OffsetTargetPrepayment = 0,
                OtherExpensesCost = 0,
                PnrCost = 0,
                SmrCost = 0
            };           

            var col = FindCellByQuery(excel, "за отчетный период").FirstOrDefault();
            if (col.Item2 == null) return null;

            for (var i = 0; i < query.Count; i++)
            {
                var find = FindCellByQuery(excel, query[i].ToArray());
                foreach (var item in find)
                {
                    (string, int) ob;
                    ob.Item1 = excel.Cells[item.Item1, item.Item2].Value.ToString();
                    ob.Item2 = item.Item1;
                    listString.Add(ob);
                }
            }

            var additionalWorkCoordates = listString.Where(x => FindByWords(x.Item1, "стоимость дополнительн работ", "стоимость доп работ")).ToList();
            var additionalNDSWorkCoordates = listString.Where(x => FindByWords(x.Item1, "сумма НДС дополнительн работ", "сумма НДС доп работ")).ToList();
            var smrCoordates = listString.Where(x => FindByWords(x.Item1, "стоимость выполненных строительно монтажных работ")).FirstOrDefault();
            var pnrCoordates = listString.Where(x => FindByWords(x.Item1, "стоимость пусконаладочных работ")).FirstOrDefault();
            var equipmentCoordates = listString.Where(x => FindByWords(x.Item1, "Стоимость оборудования")).FirstOrDefault();

            if (smrCoordates.Item1 != null)
            {
                var additionalWork = 0M;
                foreach (var item in additionalWorkCoordates)
                {
                    if (item.Item2 > smrCoordates.Item2 && item.Item2 < pnrCoordates.Item2 ||
                        item.Item2 > smrCoordates.Item2 && item.Item2 < equipmentCoordates.Item2 && pnrCoordates.Item2 == null)
                    {
                        additionalWork += (decimal)GetValueDouble(excel, item.Item2, col.Item2);
                        break;
                    }
                }
                foreach (var item in additionalNDSWorkCoordates)
                {
                    if (item.Item2 > smrCoordates.Item2 && item.Item2 < pnrCoordates.Item2 ||
                        item.Item2 > smrCoordates.Item2 && item.Item2 < equipmentCoordates.Item2 && pnrCoordates.Item2 == null)
                    {
                        additionalWork += (decimal)GetValueDouble(excel, item.Item2, col.Item2);
                        break;
                    }
                }

                var smrCost = (decimal)GetValueDouble(excel, smrCoordates.Item2, col.Item2);
                smrCost -= additionalWork;
                c3A.AdditionalCost += additionalWork;
                c3A.SmrCost = smrCost;
            }
            if (pnrCoordates.Item1 != null)
            {
                var additionalWork = 0M;
                foreach (var item in additionalWorkCoordates)
                {
                    if (item.Item2 > pnrCoordates.Item2 && item.Item2 < equipmentCoordates.Item2)
                    {
                        additionalWork += (decimal)GetValueDouble(excel, item.Item2, col.Item2);
                        break;
                    }
                }
                foreach (var item in additionalNDSWorkCoordates)
                {
                    if (item.Item2 > pnrCoordates.Item2 && item.Item2 < equipmentCoordates.Item2)
                    {
                        additionalWork += (decimal)GetValueDouble(excel, item.Item2, col.Item2);
                        break;
                    }
                }
                var pnrCost = (decimal)GetValueDouble(excel, pnrCoordates.Item2, col.Item2);
                pnrCost -= additionalWork;
                c3A.AdditionalCost += additionalWork;
                c3A.PnrCost = pnrCost;
            }
            c3A.EquipmentCost = (decimal)GetValueDouble(excel,
                listString.Where(x => FindByWords(x.Item1, "Стоимость оборудования")).FirstOrDefault().Item2, col.Item2);
            c3A.OffsetTargetPrepayment = (decimal)GetValueDouble(excel,
                listString.Where(x => FindByWords(x.Item1, "зачет целевого аванса")).FirstOrDefault().Item2, col.Item2);
            c3A.OffsetCurrentPrepayment = (decimal)GetValueDouble(excel,
                listString.Where(x => FindByWords(x.Item1, "зачет текущего аванса")).FirstOrDefault().Item2, col.Item2);
            c3A.MaterialCost = (decimal)GetValueDouble(excel,
                listString.Where(x => FindByWords(x.Item1, "материалы")).FirstOrDefault().Item2, col.Item2);
            c3A.GenServiceCost = (decimal)GetValueDouble(excel,
                listString.Where(x => FindByWords(x.Item1, "другие генуслуги")).FirstOrDefault().Item2, col.Item2);
            c3A.OtherExpensesCost = 0;
            foreach (var item in listString.Where(x => FindByWords(x.Item1, "другие", "возмещение стоимости", "возврат стоимости")))
            {
                if (!(FindByWords(item.Item1, "другие генуслуги")))
                {
                    c3A.OtherExpensesCost += (decimal)GetValueDouble(excel,
                    item.Item2, col.Item2);
                }
            }
            return c3A;
        }
    }
}
