namespace ExpRddApp.elements
{
    public class OldPlatform : SLElement
    {
        public OldPlatform(Block block, string stattionId) : base(block, stattionId)
        {
            Error = !base.Init();
            Error = !Init();
        }
    }
}
