using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact
{
    public enum XType
    {
        @none,
        Point,
        Signal,
        AxleCounterSection,
        TrackSection,
        DetectionPoint,
        BaliseGroup,
        BlockInterface,
        Connector,
        EndOfTrack,
        FoulingPoint,
        LevelCrossing,
        StaffPassengerCrossing,
        PlatformDyn,
        Platform,
        StaffCrossing,
        SignallingLayout,
        NextStation,
        CrStart,
        CrDestination,
        PassiveSignal,
        Hht,
        ExcludeBlock
    }

    public enum RddType
    {
        @none,
        tdt,
        afl,
        at,
        bg,
        bi,
        conn,
        spst,
        ovk,
        mrk,
        spsk,
        hht,
        va,
        pr,
        trk,
        fp
    }

    public enum ExtType
    {
        @none,
        derailer,
        dynamicBufferStop,
        endOfTrack,
        nonDynamicBufferStop,
        mb,
        eotmb,
        L2EntrySignal,
        L2ExitSignal,
        foreignSignal,
        point,
        trapPoint,
        singleSlipPoint,
        doubleSlipPoint,
        DoubleTrack,
        SingleTrack
    }

    public enum VertNumber
    {
        start,
        end
    }

    public enum LcTracks
    {
        @Single,
        Double
    }

    public enum SpeedType
    {
        AlC2,
        Fp,
        Fg,
        Cd100,
        Cd130,
        Cd150,
        Cd165,
        Cd180,
        Cd210,
        Cd225,
        Cd245,
        Cd275,
        Cd300,
    }
}
