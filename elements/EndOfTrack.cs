namespace ExpRddApp.elements
{
    public class EndOfTrack : SLElement
    {
        public DirectionType Direction { get; set; }
        public KindOfEOTType KindOfEOT { get; set; }
        public EndOfTrack(Block block, string stattionId) : base(block, stattionId)
        {
            Error = !base.Init();
            Error = !Init();
        }

        public override bool Init()
        {
            bool error = false;

            if (!decimal.TryParse(Attributes["KMP"].value, out decimal kmp))
            {
                ErrLogger.Error("Unable to parse OKMP1 value from attribute", ElType.ToString(), this.Designation);
                error = true;
            }
            Location = kmp;
            KindOfEOT = GetKindOfEOT();
            return !error;
        }

        private KindOfEOTType GetKindOfEOT()
        {
            if (Block.BlockReference.BlockName.Contains("DBS"))
            {
                return KindOfEOTType.dynamicBufferStop;
            }
            else if (Block.BlockReference.BlockName.Contains("NDY"))
            {
                return KindOfEOTType.nonDynamicBufferStop;
            }
            else
            {
                return KindOfEOTType.endOfTrack;
            }
        }
    }
}
