using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ExpPt1
{
    public static class WriteData
    {
        //public static string CsvName { get; set; }
        //public static string TmpId { get; set; }
        //public static string SheetId { get; set; }
        //public static string TemplatesPath { get; set; }
        //public static string DwgPath { get; set; }

        public static void ReportExcel(RailwayDesignDataMetaData metaData, string fileName)
        {
            using (var spreadsheet = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
            {
                spreadsheet.AddWorkbookPart();
                spreadsheet.WorkbookPart.Workbook = new Workbook();
                var wsPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
                wsPart.Worksheet = new Worksheet();

                var stylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = new Stylesheet
                {

                    // blank font list
                    Fonts = new Fonts()
                };
                stylesPart.Stylesheet.Fonts.AppendChild(new Font { FontName = new FontName { Val = "Arial" } });

                Font font = new Font
                {
                    Bold = new Bold(),
                    FontSize = new FontSize { Val = 12 },
                    FontName = new FontName { Val = "Arial" }
                };
                stylesPart.Stylesheet.Fonts.AppendChild(font);
                font = new Font
                {
                    FontSize = new FontSize { Val = 10 },
                    FontName = new FontName { Val = "Arial" }
                };
                stylesPart.Stylesheet.Fonts.AppendChild(font);
                font = new Font
                {
                    FontSize = new FontSize { Val = 10 },
                    Color = new Color { Rgb = HexBinaryValue.FromString("FF9C0006") },
                    FontName = new FontName { Val = "Arial" }
                };
                stylesPart.Stylesheet.Fonts.AppendChild(font);

                var solidRed = new PatternFill() { PatternType = PatternValues.Solid };
                solidRed.ForegroundColor = new ForegroundColor { Rgb = HexBinaryValue.FromString("FFFFC7CE") }; 
                solidRed.BackgroundColor = new BackgroundColor { Indexed = 64 };
                // create fills
                stylesPart.Stylesheet.Fills = new Fills();

                stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } }); // required, reserved by Excel
                stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } }); // required, reserved by Excel
                stylesPart.Stylesheet.Fills.AppendChild(new Fill { PatternFill = solidRed });

                // blank border list
                stylesPart.Stylesheet.Borders = new Borders();
                stylesPart.Stylesheet.Borders.AppendChild(new Border());
                Border border = new Border { BottomBorder = new BottomBorder { Style = BorderStyleValues.Medium, Color = new Color { Indexed = 64 } } };
                stylesPart.Stylesheet.Borders.AppendChild(border);

                // blank cell format list
                stylesPart.Stylesheet.CellStyleFormats = new CellStyleFormats();
                stylesPart.Stylesheet.CellStyleFormats.AppendChild(new CellFormat());

                // cell format list
                stylesPart.Stylesheet.CellFormats = new CellFormats();
                // empty one for index 0, seems to be required
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat());
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat
                {
                    FormatId = 0,
                    FontId = 1,
                    BorderId = 1,
                }).AppendChild(new Alignment
                {
                    Horizontal = HorizontalAlignmentValues.Center
                });
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat
                {
                    FormatId = 0,
                    FontId = 2,
                    BorderId = 0,
                }).AppendChild(new Alignment
                {
                    Horizontal = HorizontalAlignmentValues.Left
                });
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat
                {
                    FormatId = 0,
                    FontId = 2,
                    BorderId = 0,
                }).AppendChild(new Alignment
                {
                    Horizontal = HorizontalAlignmentValues.Center
                });
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat
                {
                    FormatId = 0,
                    FontId = 3,
                    BorderId = 0,
                    FillId = 2
                }).AppendChild(new Alignment
                {
                    Horizontal = HorizontalAlignmentValues.Left
                });

                stylesPart.Stylesheet.Save();

                Columns columns = new Columns();
                Column column1 = new Column() { Min = 1U, Max = 1U, Width = 50D, CustomWidth = true };
                columns.Append(column1);
                column1 = new Column() { Min = 2U, Max = 2U, Width = 22D, CustomWidth = true };
                columns.Append(column1);
                column1 = new Column() { Min = 3U, Max = 3U, Width = 10D, CustomWidth = true };
                columns.Append(column1);
                wsPart.Worksheet.AppendChild(columns);
                var sheetData = wsPart.Worksheet.AppendChild(new SheetData());

                var row = sheetData.AppendChild(new Row { RowIndex = 1, Height = 27, CustomHeight = true, ThickBot = true });
                row.AppendChild(new Cell() { CellValue = new CellValue("Doc Name"), DataType = CellValues.String, StyleIndex = 1, CellReference = "A1" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Doc Number"), DataType = CellValues.String, StyleIndex = 1, CellReference = "B1" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Version"), DataType = CellValues.String, StyleIndex = 1, CellReference = "C1" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Date"), DataType = CellValues.String, StyleIndex = 1, CellReference = "D1" });

                uint index = 2;
                uint style = 2;
                row = sheetData.AppendChild(new Row { RowIndex = index, ThickBot = true });
                row.AppendChild(new Cell() { CellValue = new CellValue(metaData.FileDescription.title), DataType = CellValues.String, StyleIndex = style, CellReference = "A" + index });
                row.AppendChild(new Cell() { CellValue = new CellValue(metaData.FileDescription.docID), DataType = CellValues.String, StyleIndex = style, CellReference = "B" + index });
                row.AppendChild(new Cell() { CellValue = new CellValue(metaData.FileDescription.version), DataType = CellValues.String, StyleIndex = style, CellReference = "C" + index });
                row.AppendChild(new Cell() { CellValue = new CellValue(metaData.FileDescription.date.ToString("d")), DataType = CellValues.String, StyleIndex = style, CellReference = "D" + index });
                index++;
                row = sheetData.AppendChild(new Row { RowIndex = index, ThickBot = true });
                row.AppendChild(new Cell() { CellValue = new CellValue(metaData.SignallingLayout.title), DataType = CellValues.String, StyleIndex = style, CellReference = "A" + index });
                row.AppendChild(new Cell() { CellValue = new CellValue(metaData.SignallingLayout.docID), DataType = CellValues.String, StyleIndex = style, CellReference = "B" + index });
                row.AppendChild(new Cell() { CellValue = new CellValue(metaData.SignallingLayout.version), DataType = CellValues.String, StyleIndex = style, CellReference = "C" + index });
                row.AppendChild(new Cell() { CellValue = new CellValue(metaData.SignallingLayout.date.ToString("d")), DataType = CellValues.String, StyleIndex = style, CellReference = "D" + index });
                index++;
                foreach (var doc in metaData.Documents.Document)
                {
                    row = sheetData.AppendChild(new Row { RowIndex = index, ThickBot = true });
                    row.AppendChild(new Cell() { CellValue = new CellValue(doc.title), DataType = CellValues.String, StyleIndex = style, CellReference = "A" + index });
                    row.AppendChild(new Cell() { CellValue = new CellValue(doc.docID), DataType = CellValues.String, StyleIndex = style, CellReference = "B" + index });
                    row.AppendChild(new Cell() { CellValue = new CellValue(doc.version), DataType = CellValues.String, StyleIndex = style, CellReference = "C" + index });
                    row.AppendChild(new Cell() { CellValue = new CellValue(doc.date.ToString("d")), DataType = CellValues.String, StyleIndex = style, CellReference = "D" + index });
                    index++;
                }
                

                SheetView sheetView = new SheetView { TabSelected = true, WorkbookViewId = 0 };
                //Selection sel = new Selection { Pane = PaneValues.BottomLeft, ActiveCell = "F" + (index -1) };
                //sel.SequenceOfReferences = new ListValue<StringValue> { Items = { "F" + (index - 1) } };             
                wsPart.Worksheet.SheetViews = new SheetViews(sheetView);
                Pane pane = new Pane { VerticalSplit = 1D, TopLeftCell = "A2", ActivePane = PaneValues.BottomLeft, State = PaneStateValues.Frozen };
                sheetView.Append(pane);
                //sheetView.Append(sel);

                wsPart.Worksheet.Save();

                var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
                sheets.AppendChild(new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(wsPart), SheetId = 1, Name = "Rdd Input Report" });
                spreadsheet.WorkbookPart.Workbook.Save();

            }
        }    
    }
}
