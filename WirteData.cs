using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Linq;

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
                sheets.AppendChild(new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(wsPart), SheetId = 1, Name = "Rdd Input" });
                spreadsheet.WorkbookPart.Workbook.Save();
            }
        }

        public static void ExpRoutes(List<RoutesRoute> routes, string fileName)
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
                stylesPart.Stylesheet.Fonts.AppendChild(new Font { FontName = new FontName { Val = "Calibri" } });

                Font font = new Font
                {
                    Bold = new Bold(),
                    FontSize = new FontSize { Val = 9 },
                    FontName = new FontName { Val = "Calibri" }
                };
                stylesPart.Stylesheet.Fonts.AppendChild(font);
                font = new Font
                {
                    FontSize = new FontSize { Val = 9 },
                    FontName = new FontName { Val = "Calibri" }
                };
                stylesPart.Stylesheet.Fonts.AppendChild(font);
                font = new Font
                {
                    FontSize = new FontSize { Val = 9 },
                    Color = new Color { Rgb = HexBinaryValue.FromString("FF9C0006") },
                    FontName = new FontName { Val = "Calibri" }
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
                Border border = new Border { BottomBorder = new BottomBorder { Style = BorderStyleValues.Medium, Color = new Color { Indexed = 64 } },
                                             TopBorder = new TopBorder { Style = BorderStyleValues.Medium, Color = new Color { Indexed = 64 } },
                                             LeftBorder = new LeftBorder{ Style = BorderStyleValues.Medium, Color = new Color { Indexed = 64 } },
                                             RightBorder = new RightBorder { Style = BorderStyleValues.Medium, Color = new Color { Indexed = 64 } }
                };
                stylesPart.Stylesheet.Borders.AppendChild(border);
                border = new Border
                {
                    BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin, Color = new Color { Indexed = 64 } },
                    TopBorder = new TopBorder { Style = BorderStyleValues.Thin, Color = new Color { Indexed = 64 } },
                    LeftBorder = new LeftBorder { Style = BorderStyleValues.Thin, Color = new Color { Indexed = 64 } },
                    RightBorder = new RightBorder { Style = BorderStyleValues.Thin, Color = new Color { Indexed = 64 } }
                };
                stylesPart.Stylesheet.Borders.AppendChild(border);

                // blank cell format list
                stylesPart.Stylesheet.CellStyleFormats = new CellStyleFormats();
                stylesPart.Stylesheet.CellStyleFormats.AppendChild(new CellFormat());

                // cell format list
                stylesPart.Stylesheet.CellFormats = new CellFormats();
                // empty one for index 0, seems to be required
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat());

                //style 1
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat
                {
                    FormatId = 0,
                    FontId = 1,
                    BorderId = 1,
                }).AppendChild(new Alignment
                {
                    Horizontal = HorizontalAlignmentValues.Center,
                    Vertical = VerticalAlignmentValues.Center,
                    WrapText = true
                });

                //style 2
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat
                {
                    FormatId = 0,
                    FontId = 2,
                    BorderId = 2,
                }).AppendChild(new Alignment
                {
                    Horizontal = HorizontalAlignmentValues.Center,
                    Vertical = VerticalAlignmentValues.Center
                });

                //style 3
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat
                {
                    FormatId = 0,
                    FontId = 2,
                    BorderId = 0,
                }).AppendChild(new Alignment
                {
                    Horizontal = HorizontalAlignmentValues.Center
                });

                //style 4
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

                //style 5
                stylesPart.Stylesheet.CellFormats.AppendChild(new CellFormat
                {
                    FormatId = 0,
                    FontId = 2,
                    BorderId = 2,
                }).AppendChild(new Alignment
                {
                    Horizontal = HorizontalAlignmentValues.Center,
                    Vertical = VerticalAlignmentValues.Center,
                    WrapText = true
                });

                stylesPart.Stylesheet.Save();

                Columns columns = new Columns();
                Column column1 = new Column() { Min = 1U, Max = 1U, Width = 4.8D, CustomWidth = true };
                columns.Append(column1);
                column1 = new Column() { Min = 2U, Max = 2U, Width = 10.57D, CustomWidth = true };
                columns.Append(column1);
                column1 = new Column() { Min = 3U, Max = 3U, Width = 15D, CustomWidth = true };
                columns.Append(column1);
                column1 = new Column() { Min = 4U, Max = 4U, Width = 15D, CustomWidth = true };
                columns.Append(column1);
                column1 = new Column() { Min = 5U, Max = 5U, Width = 10.8D, CustomWidth = true };
                columns.Append(column1);
                column1 = new Column() { Min = 6U, Max = 6U, Width = 7.15D, CustomWidth = true };
                columns.Append(column1);
                column1 = new Column() { Min = 7U, Max = 8U, Width = 13.6D, CustomWidth = true };
                columns.Append(column1);
                column1 = new Column() { Min = 8U, Max = 11U, Width = 12D, CustomWidth = true };
                columns.Append(column1);
                column1 = new Column() { Min = 12U, Max = 12U, Width = 13.6D, CustomWidth = true };
                columns.Append(column1);
                column1 = new Column() { Min = 13U, Max = 13U, Width = 30.58D, CustomWidth = true };
                columns.Append(column1);
                column1 = new Column() { Min = 14U, Max = 20U, Width = 10.3D, CustomWidth = true };
                columns.Append(column1);
                wsPart.Worksheet.AppendChild(columns);
                var sheetData = wsPart.Worksheet.AppendChild(new SheetData());

                uint index = 1;
                var row = sheetData.AppendChild(new Row { RowIndex = 1, Height = 33.75, CustomHeight = true, ThickBot = true });
                row.AppendChild(new Cell() { CellValue = new CellValue("Description of Routes"), DataType = CellValues.String, StyleIndex = 1, CellReference = "A1" });
                char col = 'B';
                for (int j = 0; j < 12; j++)
                {
                    row.AppendChild(new Cell() { CellValue = new CellValue(), DataType = CellValues.String, StyleIndex = 1, CellReference = col + "1" });
                    col++;
                }
               
                //row = sheetData.AppendChild(new Row { RowIndex = 1, Height = 33.75, CustomHeight = true, ThickBot = true });
                row.AppendChild(new Cell() { CellValue = new CellValue("Internal Thales - Strukton"), DataType = CellValues.String, StyleIndex = 1, CellReference = "N1" });
                col = 'O';
                for (int j = 0; j < 6; j++)
                {
                    row.AppendChild(new Cell() { CellValue = new CellValue(), DataType = CellValues.String, StyleIndex = 1, CellReference = col + "1" });
                    col++;
                }
                index++;

                row = sheetData.AppendChild(new Row { RowIndex = 2, Height = 36.75, CustomHeight = true, ThickBot = true });
                row.AppendChild(new Cell() { CellValue = new CellValue("Route\r\nno."), DataType = CellValues.String, StyleIndex = 1, CellReference = "A2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Type of Route"), DataType = CellValues.String, StyleIndex = 1, CellReference = "B2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Start of Route\r\n(from MB)"), DataType = CellValues.String, StyleIndex = 1, CellReference = "C2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("End Of Route\r\n(to MB)"), DataType = CellValues.String, StyleIndex = 1, CellReference = "D2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Preferred\r\nroute"), DataType = CellValues.String, StyleIndex = 1, CellReference = "E2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Via points"), DataType = CellValues.String, StyleIndex = 1, CellReference = "F2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Activate crossing element group"), DataType = CellValues.String, StyleIndex = 1, CellReference = "G2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Extended destination area"), DataType = CellValues.String, StyleIndex = 1, CellReference = "H2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Flank protection"), DataType = CellValues.String, StyleIndex = 1, CellReference = "I2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Add. Tdt"), DataType = CellValues.String, StyleIndex = 1, CellReference = "J2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Overlap"), DataType = CellValues.String, StyleIndex = 1, CellReference = "K2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Short overlap"), DataType = CellValues.String, StyleIndex = 1, CellReference = "L2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Notes"), DataType = CellValues.String, StyleIndex = 1, CellReference = "M2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Point Group"), DataType = CellValues.String, StyleIndex = 1, CellReference = "N2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Safety Distance designation"), DataType = CellValues.String, StyleIndex = 1, CellReference = "O2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Sd Last Element"), DataType = CellValues.String, StyleIndex = 1, CellReference = "P2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Ovl Timer Start Loc"), DataType = CellValues.String, StyleIndex = 1, CellReference = "Q2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Start Area Group"), DataType = CellValues.String, StyleIndex = 1, CellReference = "R2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Excluded Routes"), DataType = CellValues.String, StyleIndex = 1, CellReference = "S2" });
                row.AppendChild(new Cell() { CellValue = new CellValue("Remarks"), DataType = CellValues.String, StyleIndex = 1, CellReference = "T2" });
                index++;

                uint style = 2;
                foreach (var rt in routes)
                {
                    row = sheetData.AppendChild(new Row { RowIndex = index, ThickBot = true, Height = 33.75, CustomHeight = true });
                    row.AppendChild(new Cell() { CellValue = new CellValue((index - 2).ToString()), DataType = CellValues.String, StyleIndex = style, CellReference = "A" + index });
                    row.AppendChild(new Cell() { CellValue = new CellValue(), DataType = CellValues.String, StyleIndex = style, CellReference = "B" + index });
                    row.AppendChild(new Cell() { CellValue = new CellValue(rt.Start), DataType = CellValues.String, StyleIndex = style, CellReference = "C" + index });
                    row.AppendChild(new Cell() { CellValue = new CellValue(rt.Destination), DataType = CellValues.String, StyleIndex = style, CellReference = "D" + index });
                    col = 'E';
                    for (int j = 0; j < 9; j++)
                    {
                        row.AppendChild(new Cell() { CellValue = new CellValue(), DataType = CellValues.String, StyleIndex = style, CellReference = col + index.ToString() });
                        col++;
                    }
                    List<string> points = new List<string>();
                    if (rt.PointGroup.Point.Count() > 0)
                    {                     
                        foreach (var pt in rt.PointGroup.Point)
                        {
                            points.Add(pt.Value.Split('-')[2] + "-" + pt.RequiredPosition.ToString().First().ToString().ToUpper());
                        }
                    }
                    row.AppendChild(new Cell() { CellValue = new CellValue(string.Join("\r\n", points)), DataType = CellValues.String, StyleIndex = 5, CellReference = "N" + index });
                    col = 'O';
                    for (int j = 0; j < 6; j++)
                    {
                        row.AppendChild(new Cell() { CellValue = new CellValue(), DataType = CellValues.String, StyleIndex = style, CellReference = col + index.ToString() });
                        col++;
                    }
                    index++;
                }

                MergeCells mergeCells = new MergeCells();
                mergeCells.Append(new MergeCell() { Reference = new StringValue("A1:M1") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue("N1:T1") });
                wsPart.Worksheet.InsertAfter(mergeCells, wsPart.Worksheet.Elements<SheetData>().First());

                SheetView sheetView = new SheetView { TabSelected = true, WorkbookViewId = 0 };
                //Selection sel = new Selection { Pane = PaneValues.BottomLeft, ActiveCell = "F" + (index -1) };
                //sel.SequenceOfReferences = new ListValue<StringValue> { Items = { "F" + (index - 1) } };             
                wsPart.Worksheet.SheetViews = new SheetViews(sheetView);
                Pane pane = new Pane { VerticalSplit = 1D, TopLeftCell = "A3", ActivePane = PaneValues.BottomLeft, State = PaneStateValues.Frozen };
                //sheetView.Append(pane);
                //sheetView.Append(sel);

                wsPart.Worksheet.Save();

                var sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
                sheets.AppendChild(new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(wsPart), SheetId = 1, Name = "Routes" });
                spreadsheet.WorkbookPart.Workbook.Save();
            }
        }
    }
}
