using OfficeOpenXml.Style;
using BusinessLayer.Helpers;
using OfficeOpenXml;
using System.Drawing;
using BusinessLayer.Interfaces.COMServices;

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
                _excelWorksheet = _excelWorkbook.Worksheets.Add(Constants.WORKSHEET_NAME_DEFAULT);
            }
            _excelWorksheet.DefaultColWidth = 15;
            _excelWorksheet.TabColor = Color.Black;
            _excelWorksheet.DefaultRowHeight = 14;

            return _excelWorksheet;
        }

        public bool WriteLine(ExcelWorksheet sheet, int rowLine, string path, bool isHeader = false, double? headerHeight = null, double? width = null, bool? isTextWrap = null, ExcelHorizontalAlignment? align = null, params RowItem[] values)
        {
            if (rowLine > 0 && values.Length != 0)
            {
                if (isHeader)
                {
                    sheet.Row(rowLine).Height = headerHeight.HasValue ? headerHeight.Value : 20;
                    sheet.Row(rowLine).Style.HorizontalAlignment = (align == null) ? ExcelHorizontalAlignment.Center : align.Value;
                    sheet.Row(rowLine).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    sheet.Row(rowLine).Style.Font.Bold = true;
                }

                sheet.Row(rowLine).Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                foreach (var value in values)
                {
                    sheet.Cells[rowLine, value.Col].Style.Font.Size = value.FontSize;
                    sheet.Cells[rowLine, value.Col].Style.Font.Color.SetColor(value.FontColor);

                    if (isTextWrap.HasValue)
                    {
                        sheet.Cells[rowLine, value.Col].Style.WrapText = isTextWrap.Value;
                    }

                    if (!value.BgColor.IsEmpty)
                    {
                        sheet.Cells[rowLine, value.Col].Style.Fill.SetBackground(value.BgColor);
                    }

                    sheet.Cells[rowLine, value.Col].Value = value.Value;

                    //устанавливаем ширину ячейки
                    if (value.ColWidth.HasValue)
                    {
                        sheet.Column(value.Col).Width = value.ColWidth.Value;
                    }
                    else if (width.HasValue)
                    {
                        double currentWidth = sheet.Column(value.Col).Width;
                        sheet.Column(value.Col).Width = currentWidth < width.Value ? width.Value : currentWidth;
                    }
                    else
                    {
                        sheet.Column(value.Col).AutoFit();
                    }
                }

                try
                {
                    File.WriteAllBytes(path, _excelPackage.GetAsByteArray());
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

        public bool WriteLine(ExcelWorksheet sheet, string path, ExcelSettings setting, params RowItem[] values)
        {
            if (setting.RowLine > 0 && values.Length != 0)
            {
                try
                {
                    if (setting.CellHeight.HasValue)
                    {
                        sheet.Row(setting.RowLine).Height = setting.CellHeight.Value;
                    }

                    sheet.Row(setting.RowLine).Style.HorizontalAlignment = setting.horizAligment;
                    sheet.Row(setting.RowLine).Style.VerticalAlignment = setting.vertAligment;
                    sheet.Row(setting.RowLine).Style.Font.Bold = setting.isBold ?? false;
                    
                    foreach (var value in values)
                    {
                        sheet.Cells[setting.RowLine, value.Col].Style.Font.Size = setting.FontSize;

                        if (!value.FontColor.IsEmpty)
                        {
                            sheet.Cells[setting.RowLine, value.Col].Style.Font.Color.SetColor(value.FontColor);
                        }
                        else if (!setting.FontColor.IsEmpty)
                        {
                            sheet.Cells[setting.RowLine, value.Col].Style.Font.Color.SetColor(setting.FontColor);
                        }



                        if (setting.isTextWrap.HasValue)
                        {
                            sheet.Cells[setting.RowLine, value.Col].Style.WrapText = setting.isTextWrap.Value;
                        }

                        if (!value.BgColor.IsEmpty)
                        {
                            sheet.Cells[setting.RowLine, value.Col].Style.Fill.SetBackground(value.BgColor);
                        }
                        else if (!setting.BgColor.IsEmpty)
                        {
                            sheet.Cells[setting.RowLine, value.Col].Style.Fill.SetBackground(setting.BgColor);
                        }

                        sheet.Cells[setting.RowLine, value.Col].Value = value.Value;


                        //устанавливаем ширину ячейки
                        if (value.ColWidth.HasValue)
                        {
                            sheet.Column(value.Col).Width = value.ColWidth.Value;
                        }
                        else if (setting.CellWidth.HasValue)
                        {
                            double currentWidth = sheet.Column(value.Col).Width;
                            sheet.Column(value.Col).Width = currentWidth < setting.CellWidth.Value ? setting.CellWidth.Value : currentWidth;
                        }
                        else
                        {
                            sheet.Column(value.Col).AutoFit();
                        }

                        sheet.Cells[setting.RowLine, value.Col].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        sheet.Cells[setting.RowLine, value.Col].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        sheet.Cells[setting.RowLine, value.Col].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        sheet.Cells[setting.RowLine, value.Col].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                        //объединяем ячейки в строке
                        if (setting.isMergeCell == true)
                        {
                            sheet.Cells[setting.RowLine, setting.mergeStartCell, setting.RowLine, (setting.mergeStartCell + setting.mergeCountCell - 1)].Merge = true;
                        }
                    }

                    File.WriteAllBytes(path, _excelPackage.GetAsByteArray());
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
        public int? ColWidth { get; set; }
        public int FontSize { get; set; } = 12;
        public Color FontColor { get; set; } = Color.Empty;
        public Color BgColor { get; set; } = Color.Empty;
    }

    public class ExcelSettings
    {
        public bool? isBold { get; set; }
        public bool? isItalic { get; set; }
        public bool? isUnderline { get; set; }
        public int FontSize { get; set; } = 12;
        public int? CellHeight { get; set; }
        public int? CellWidth { get; set; }
        public bool? isTextWrap { get; set; }
        public bool? isMergeCell { get; set; }
        public bool? isBorder { get; set; }
        public int mergeStartCell { get; set; }
        public int mergeCountCell { get; set; }
        public int RowLine { get; set; }

        public Color FontColor { get; set; } = Color.Black;
        public Color BgColor { get; set; }
        public ExcelHorizontalAlignment horizAligment { get; set; } = ExcelHorizontalAlignment.Left;
        public ExcelVerticalAlignment vertAligment { get; set; } = ExcelVerticalAlignment.Top;
    }
}
