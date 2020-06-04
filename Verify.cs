using System.Collections.Generic;
using System.Linq;

namespace ExpPt1
{
    public class Verify
    {
        public void CheckSSPsSegments(RailwayDesignData rdd, List<TrackSegmentTmp> trackSegments)
        {
            var trackSegmentsSSPs = rdd.SpeedProfiles.SpeedProfile.SelectMany(x => x.TrackSegments.TrackSegment).Select(t => t.Value).ToList();
            var trackSegmentsDesigs = trackSegments.Select(x => x.Designation).ToList();
            foreach (string ssp in trackSegmentsSSPs)
            {
                if (!trackSegmentsDesigs.Contains(ssp))
                {
                    ErrLogger.Warning("Segment not found in track segments", "SSP verification", ssp);
                }
            }
        }
    }
}
