namespace ExpRddApp
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

    public enum GetCommVertOptions
    {
        byFirstSeg,
        bySecondSeg
    }
}
