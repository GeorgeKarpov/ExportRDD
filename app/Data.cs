using System.Collections.Generic;
using System.Data;

namespace ExpRddApp
{
    public static class Data
    {
        public static DataTable ToDataTable(List<elements.TSeg> tsegs)
        {
            DataTable dataTable = new DataTable("Segments");
            dataTable.Columns.Add("Designation", typeof(string));
            dataTable.Columns.Add("Line", typeof(string));
            dataTable.Columns.Add("Vertex 1", typeof(string));
            dataTable.Columns.Add("Vertex 2", typeof(string));
            foreach (var item in tsegs)
            {
                var values = new object[4];
                values[0] = item.Id;
                values[1] = item.LineID;
                values[2] = item.Vertex1.Id;
                values[3] = item.Vertex2.Id; ;
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static DataTable ToDataTable(List<elements.Signal> items)
        {
            DataTable dataTable = new DataTable("Signals");
            dataTable.Columns.Add("Designation", typeof(string));
            dataTable.Columns.Add("Kind", typeof(string));
            dataTable.Columns.Add("Location", typeof(string));
            dataTable.Columns.Add("Track Segment", typeof(string));
            dataTable.Columns.Add("Line", typeof(string));
            dataTable.Columns.Add("Danger Point Id", typeof(string));
            dataTable.Columns.Add("Danger Point Distance", typeof(decimal));
            dataTable.Columns.Add("Shift Oces", typeof(decimal));
            foreach (var item in items)
            {
                var values = new object[8];
                var dangPoint = item.DangerPoint;
                values[0] = item.Designation;
                values[1] = item.KindOfSignal;
                values[2] = item.Location;
                values[3] = item.GetTsegId();
                values[4] = item.LineID;
                values[5] = item.DangerPoint.Id;
                values[6] = item.DangerPoint.Distance;
                values[7] = item.GetShiftOces();
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static DataTable ToDataTable(List<elements.Point> items)
        {
            DataTable dataTable = new DataTable("Points");
            dataTable.Columns.Add("Designation", typeof(string));
            dataTable.Columns.Add("Kind", typeof(string));
            dataTable.Columns.Add("Location", typeof(string));
            //dataTable.Columns.Add("Track Segment", typeof(string));
            //dataTable.Columns.Add("Line", typeof(string));
            foreach (var item in items)
            {
                var values = new object[3];
                values[0] = item.Designation;
                values[1] = item.Kind();
                values[2] = item.KmpTip;
                //values[3] = item.TrackSegmentID;
                //values[4] = item.LineID;
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static DataTable ToDataTable(List<ExcelLib.ExpRoute> items)
        {
            DataTable dataTable = new DataTable("Routes");
            dataTable.Columns.Add("Start", typeof(string));
            dataTable.Columns.Add("Destination", typeof(string));
            //dataTable.Columns.Add("Destination", typeof(string));
            //dataTable.Columns.Add("Track Segment", typeof(string));
            //dataTable.Columns.Add("Line", typeof(string));
            foreach (var item in items)
            {
                var values = new object[2];
                //values[0] = item.Designation;
                values[0] = item.Start;
                values[1] = item.Destination;
                //values[3] = item.TrackSegmentID;
                //values[4] = item.LineID;
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static DataTable ToDataTable(elements.SigLayout siglayout)
        {
            DataTable dataTable = new DataTable("Sig Layout");
            dataTable.Columns.Add("Title", typeof(string));
            dataTable.Columns.Add("Doc ID", typeof(string));
            dataTable.Columns.Add("Version", typeof(string));
            dataTable.Columns.Add("Date", typeof(string));
            dataTable.Columns.Add("Creator", typeof(string));
            var values = new object[5];
            values[0] = siglayout.Title;
            values[1] = siglayout.DocId;
            values[2] = siglayout.Version;
            values[3] = siglayout.Date.ToString("dd.MM.yyyy");
            values[4] = siglayout.Creator;
            dataTable.Rows.Add(values);
            return dataTable;
        }
    }
}
