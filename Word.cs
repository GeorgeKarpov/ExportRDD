using Microsoft.Office.Interop.Word;
using System;
using System.IO;

namespace ReadWord
{
    /// <summary>
    /// Reads MS Word documents using Microsoft.Office.Interop.Word.
    /// </summary>
    class Word
    {
        public TFileDescr GetCoverData(string path, ref bool error)
        {
            TFileDescr CoverPage = new TFileDescr();
            Application application = new Application();
            Document document = new Document();
            try
            {
                document =
                application.Documents.Open(path);
            }
            catch
            {
                application.Quit();
                error = true;
                ErrLogger.Log("Can not open document: " + Path.GetFileName(path));
                return CoverPage;
            }
            Table table =
                    document.Sections[1].Footers[WdHeaderFooterIndex.wdHeaderFooterFirstPage].Range.Tables[1];
            Range range = table.Range;
            bool found1 = false;
            bool found2 = false;
            for (int i = 1; i <= range.Cells.Count; i++)
            {
                if (range.Cells[i].RowIndex == 1 && range.Cells[i].ColumnIndex == 3)
                {
                    CoverPage.docID = range.Cells[i].Range.Text
                                      .Split(':')[1]
                                      .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]
                                      .Trim().Replace("\r\a", "");
                    CoverPage.version = range.Cells[i].Range.Text
                                      .Split(':')[1]
                                      .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]
                                      .Trim().Replace("/", "")
                                      .Replace("v", "").Replace("\r\a", "");
                    found1 = true;
                }

                if (range.Cells[i].RowIndex == 11 && range.Cells[i].ColumnIndex == 3)
                {
                    DateTime docDate = DateTime.MinValue;
                    string s = range.Cells[i].Range.Text
                                    .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]
                                    .Trim().Replace("\r\a", "");
                    ExpPt1.Calc.StringToDate(s, out docDate, out bool datconv, false);
                    if (!datconv)
                    {
                        for (int j = 1; j <= range.Cells.Count; j++)
                        {
                            if (range.Cells[j].RowIndex == 11 && range.Cells[j].ColumnIndex == 2)
                            {
                                s = range.Cells[j].Range.Text
                                    .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]
                                    .Trim().Replace("\r\a", "");
                                ExpPt1.Calc.StringToDate(s, out docDate, out datconv);
                                if (!datconv)
                                {
                                    error = true;
                                    ErrLogger.Log("Cover Page: cannot get date and author" + document.Name);
                                    break;
                                }
                                else
                                {
                                    CoverPage.date = docDate;
                                    CoverPage.creator = range.Cells[j].Range.Text
                                                    .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]
                                                    .Trim().Replace("\r\a", "");
                                    found2 = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        CoverPage.date = docDate;
                        CoverPage.creator = range.Cells[i].Range.Text
                                        .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]
                                        .Trim().Replace("\r\a", "");
                        found2 = true;
                    }
                }

                if (found1 && found2)
                {
                    application.Quit();
                    return CoverPage;
                }
            }
            application.Quit();
            return CoverPage;
        }
    }
}
