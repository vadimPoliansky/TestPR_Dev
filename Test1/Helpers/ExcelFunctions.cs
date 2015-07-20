using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClosedXML.Excel;

namespace IndInv.Helpers
{
    public class ExcelFunctions
    {
        public static void AutoFitWorksheet(IXLWorksheet ws, int startCol, int startRow, double lineHeight)
        {
            foreach (var row in ws.RowsUsed().Skip(startRow))
            {
                var maxLines = 1.0;
                foreach (var cell in row.CellsUsed().Skip(startCol))
                {
                    if (cell.Value != null)
                    {
                        var lines = cell.Value.ToString().Length / (cell.WorksheetColumn().Width * 1.15);
                        maxLines = maxLines < lines ? lines : maxLines;
                    }
                }
                row.Height = lineHeight * Math.Ceiling(maxLines);
            }
        }

        public static double getTotalHeight(IXLWorksheet ws, int startRow)
        {
            var totalHeight = 0.0;
            foreach (var row in ws.Rows(startRow, ws.LastRowUsed().RowNumber()))
            {
                totalHeight += row.Height;
            }

            return totalHeight;

        }

        public static double getTotalWidth(IXLWorksheet ws, int startCol)
        {
            var totalWidth = 0.0;
            foreach (var col in ws.Columns(startCol, ws.LastColumnUsed().ColumnNumber()))
            {
                totalWidth += col.Width * 5.69;
            }

            return totalWidth;
        }
    }
}