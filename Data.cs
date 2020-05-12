using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace ExpPt1
{
    public static class Data
    {
        public static DataTable SegsToDataTable(List<TrackSegmentsTrackSegment> items)
        {
            DataTable dataTable = new DataTable("Segments");
            dataTable.Columns.Add("Designation", typeof(string));
            dataTable.Columns.Add("Line", typeof(string));
            dataTable.Columns.Add("Vertex 1", typeof(string));
            dataTable.Columns.Add("Vertex 2", typeof(string));
            foreach (var item in items)
            {
                var values = new object[4];
                values[0] = item.Designation;
                values[1] = item.LineID;
                values[2] = item.Vertex1.vertexID;
                values[3] = item.Vertex2.vertexID; ;             
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static DataTable SignalsToDataTable(List<SignalsSignal> items)
        {
            DataTable dataTable = new DataTable("Signals");
            dataTable.Columns.Add("Designation", typeof(string));
            dataTable.Columns.Add("Kind", typeof(string));
            dataTable.Columns.Add("Location", typeof(string));
            dataTable.Columns.Add("Track Segment", typeof(string));
            dataTable.Columns.Add("Line", typeof(string));
            foreach (var item in items)
            {
                var values = new object[5];
                values[0] = item.Designation;
                values[1] = item.KindOfSignal;
                values[2] = item.Location;
                values[3] = item.TrackSegmentID;
                values[4] = item.LineID;
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static DataTable PointsToDataTable(List<PointsPoint> items)
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
                values[1] = item.KindOfPoint;
                values[2] = item.Lines.Line[0].Location;
                //values[3] = item.TrackSegmentID;
                //values[4] = item.LineID;
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static DataTable SigLayouToDataTable(TFileDescr siglayout)
        {
            DataTable dataTable = new DataTable("Sig Layout");
            dataTable.Columns.Add("Title", typeof(string));
            dataTable.Columns.Add("Doc ID", typeof(string));
            dataTable.Columns.Add("Version", typeof(string));
            dataTable.Columns.Add("Date", typeof(string));
            dataTable.Columns.Add("Creator", typeof(string));
            var values = new object[5];
            values[0] = siglayout.title;
            values[1] = siglayout.docID;
            values[2] = siglayout.version;
            values[3] = siglayout.date.ToString("dd.MM.yyyy");
            values[4] = siglayout.creator;
            dataTable.Rows.Add(values);
            return dataTable;
        }
    }
}
