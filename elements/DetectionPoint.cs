namespace ExpRddApp.elements
{
    public class DetectionPoint : SLElement
    {
        public KindOfDPType KindOfDP { get; set; } = KindOfDPType.axleCounter;
        public DetectionPoint(Block block, string stattionId) : base(block, stattionId)
        {
            Error = !base.Init();
            Error = !Init();
        }

        public override bool Init()
        {
            bool error = false;
            if (!decimal.TryParse(Attributes["KMP"].value, out decimal km))
            {
                ErrLogger.Error("Unable to parse KMP value from attribute", this.ElType.ToString(), this.Designation);
                error = true;
            }
            this.Location = km;
            return error;
        }
    }
}
