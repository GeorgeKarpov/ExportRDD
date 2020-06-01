namespace ExpPt1
{
    /// <summary>
    /// Constants class.
    /// </summary>
    public static class Constants
    {
        public const int yTol = 1; // non-intersected lines tolerance finding track segments path.
        public const double psaTol = 1.0;
        public const int minTrLineLength = 2; // minimum track segment line length.
        public const string defaultFileName = "ExportRdd"; // default output file name.
        public const int nextNodeMaxAttemps = 99;
        public const int nextAcMaxAttemps = 20;
        public const int maxRoutesInCmRoute = 8;
        public const int maxCmpRoutesIteraion = 99;
        public const int maxSxDelays = 20;
        public const string tdtNameRegexp = "tdt-[a-zæøåÆØÅA-Z]{2,3}-[0-9]{3}";
        public const int dpIterLimit = 9;
        public const int toPsaIterLimit = 19;
        public const string cfgFolder = @"/cfg";
        public const string logFolder = @"/log";
    }
}
