using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpPt1
{
    public class Verify
    {
        public void CheckSSPsSegments(RailwayDesignData rdd)
        {
            var trackSegmentsSSPs = rdd.SpeedProfiles.SpeedProfile.SelectMany(x => x.TrackSegments.TrackSegment).Select(t => t.Value).ToList();
            var trackSegmentsRDDs = rdd.TrackSegments.TrackSegment.Select(x => x.Designation).ToList();
            foreach (string ssp in trackSegmentsSSPs)
            {
                if (!trackSegmentsRDDs.Contains(ssp))
                {
                    ErrLogger.Log("SSP segment '" + ssp + "' not found in track segments");
                }
            }
        }
    }
}
