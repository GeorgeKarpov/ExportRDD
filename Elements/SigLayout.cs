using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExportV2
{
    public class SigLayout : SLElement
    {
        public TFileDescr siglayout;
        public string DocId { get; set; }
        public string DocVrs { get; set; }
        private string title;
        private string creator;
        private DateTime date;

        public SigLayout(Block block, string BlkMap)
        {
            Block = block;
            blkMap = BlkMap;
            InitError = !Init();
        }

        public override bool Init()
        {
            bool error = false;
            error = !base.Init();

            string tmpCreator = this.Attributes
                       .Where(x => x.Key.Contains("KONSTRUERET") && x.Value.Value != "")
                       .Select(y => y.Value.Value)
                       .FirstOrDefault();
            if (tmpCreator.Split(null).Length > 0)
            {
                creator = tmpCreator
                    .Split(new Char[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            }
            date =
                    Convert.ToDateTime(
                        Regex.
                        Split(this.Attributes["UDGAVE"].Value, @"\s{1,}")[1]);
            title = this.Attributes["2-TEGN.NAVN"].Value + " - " +
                              this.Attributes["1-ST.NAVN"].Value;
            GetDocIdVrs();


            char[] split = new char[] { '-', '(' };
            StName =
                this.Attributes["1-ST.NAVN"].Value.Split(split)[0].TrimEnd(')').Trim();
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            StName = textInfo.ToTitleCase(StName.ToLower());
            this.StId = this.Attributes["1-ST.NAVN"].Value
                                  .Split(new char[] { '(', '-' }, StringSplitOptions.RemoveEmptyEntries)[1]
                                  .Split(new char[] { ')', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]
                                  .Trim()
                                  .ToLower();
            if (this.StId.Length == 0)
            {
                ErrLogger.Log("Station ID is empty");
                error = false;
            }
            return !error;
        }

        private void GetDocIdVrs()
        {
            Database db = this.Block.BlkRef.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var Textsids = AcadTools.GetObjectsOfType(db, RXObject.GetClass(typeof(MText)));
                foreach (ObjectId ObjId in Textsids)
                {
                    var mtext = (MText)trans.GetObject(ObjId, OpenMode.ForRead);
                    if (mtext.Text.Contains("Internt Thales tegningsnr"))
                    {
                        DocId = mtext.Text.Split(':')[1].Trim().Split(' ')[0];
                        DocVrs = mtext.Text.Split(':')[1].Trim()
                            .Split(new Char[] { ' ', ',', '.', ':', '\n', '\t', '/', 'v', 'V' },
                                       StringSplitOptions.RemoveEmptyEntries).Last();
                        break;
                    }
                }
                BlockTableRecord btRecord = (BlockTableRecord)trans.GetObject(SymbolUtilityServices.GetBlockPaperSpaceId(db), OpenMode.ForRead);
                foreach (ObjectId id in btRecord)
                {
                    Entity entity = (Entity)trans.GetObject(id, OpenMode.ForRead);
                    if (entity.GetType() == typeof(MText) &&
                        ((MText)entity).Text.Contains("Internt Thales tegningsnr"))
                    {
                        var mtext = (MText)entity;
                        DocId = mtext.Text.Split(':')[1].Trim().Split(' ')[0];
                        DocVrs = mtext.Text.Split(':')[1].Trim()
                            .Split(new Char[] { ' ', ',', '.', ':', '\n', '\t', '/', 'v', 'V' },
                                       StringSplitOptions.RemoveEmptyEntries).Last();
                        //.Replace("v/", ""); ;
                        break;
                    }
                }
                trans.Commit();
            }
        }

        public override dynamic ConvertToRdd()
        {
            this.siglayout = new TFileDescr
            {
                creator = creator,
                version = DocVrs,
                docID = DocId.ToUpper(),
                date = date,
                title = title
            };
            return siglayout;
        }
    }
}
