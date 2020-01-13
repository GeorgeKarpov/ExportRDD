using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Linq;
using System.IO.Packaging;
using System.IO;

namespace ExportV2
{
    public static class WordOpenXML
    {
        public static bool DocError { get; set; } 
        public static TFileDescr GetDocument(string filePath)
        {
            DocError = false;
            string creator = "";
            string docId = "";
            string docVers = "";
            string dateString = "";
            DateTime date;
            Package wordPackage = Package.Open(filePath, FileMode.Open, FileAccess.Read);
            using (WordprocessingDocument doc = WordprocessingDocument.Open(wordPackage))
            {
                Footer footer =
                    doc.MainDocumentPart.FooterParts.LastOrDefault().Footer;
                Table table = footer.Elements<Table>().First();
                docId = table.Elements<TableRow>()
                             .ElementAt(0).Elements<TableCell>()
                             .ElementAt(2).InnerText
                             .Split(':')[1]
                             .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]
                             .Trim().Replace("\r\a", "");
                docVers = table.Elements<TableRow>()
                             .ElementAt(0).Elements<TableCell>()
                             .ElementAt(2).InnerText
                             .Split(':')[1]
                             .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]
                             .Trim().Replace("/", "")
                             .Replace("v", "").Replace("\r\a", "");
                dateString = table.Elements<TableRow>()
                             .ElementAt(10).Elements<TableCell>()
                             .ElementAt(2).InnerText
                             .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]
                             .Trim().Replace("\r\a", "");

                if (!DateTime.TryParse(dateString, out date))
                {
                    dateString = table.Elements<TableRow>()
                             .ElementAt(10).Elements<TableCell>()
                             .ElementAt(1).InnerText
                             .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]
                             .Trim().Replace("\r\a", "");
                    if (!DateTime.TryParse(dateString, out date))
                    {
                        ErrLogger.Log("Unable to parse date: '" + filePath + "'");
                        DocError = true;
                    }
                    else
                    {
                        creator = table.Elements<TableRow>()
                                  .ElementAt(10).Elements<TableCell>()
                                  .ElementAt(2).InnerText
                                  .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]
                                  .Trim().Replace("\r\a", "");
                    }
                }
                else
                {
                    creator = table.Elements<TableRow>()
                            .ElementAt(10).Elements<TableCell>()
                            .ElementAt(2).InnerText
                            .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]
                             .Trim().Replace("\r\a", "");
                }
            }
            return new TFileDescr
            {
                docID = docId,
                version = docVers,
                creator = creator,
                date = date
            };
        }
    }
}
