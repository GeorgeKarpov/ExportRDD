namespace Refact
{
    /// <summary>
    /// Constants class.
    /// </summary>
    public static class Constants
    {
        public const int yTol = 1; // non-intersected lines tolerance finding track segments path.
        public const double pointsEqTol = 0.1;
        public const double beamLineTol = 0.1;
        public const double psaTol = 1.0;
        public const int minTrLineLength = 2; // minimum track segment line length.
        public const string defaultFileName = "ExportRdd"; // default output file name.
        public const int nextNodeMaxAttemps = 29;
        public const int nextAcMaxAttemps = 20;
        public const int maxRoutesInCmRoute = 8;
        public const int maxCmpRoutesIteraion = 99;
        public const int maxSxDelays = 20;
        public const string tdtNameRegexp = "tdt-[a-zæøåÆØÅA-Z]{2,3}-[0-9]{3}";
        public const int dpIterLimit = 9;
        public const int sigIterLimit = 19;
        public const int toPsaIterLimit = 19;
        public const string cfgFolder = @"/cfg";
        public const string logFolder = @"/log";
        public const double insidePsaToler = 1.0;
        public const double mTrackDist = 6;

        public const string trackLinesLayer = "_Spor";
        public const string trustAreaLayer = "(?i)trusted.*area(?-i)";

        public const string trackRegex = @"^\s*[Ss][Pp]\s*\d{1,3}\s*$";

        public const string lxTrackLoc = @"^\s*KMP(_*TSEG\d){0,1}$\s*";
        public const string lxTrackBeginReg = @"^\s*(BEGIN|START)_*(LCA|KMP){0,1}(_*TSEG\d){0,1}$\s*";
        public const string lxTrackEndReg = @"^\s*END_*(LCA|KMP){0,1}(_*TSEG\d){0,1}$\s*";
        public const string lxTrackLengthReg = @"^\s*LENG(HT|TH)_*(LCA|\d){0,1}(_*TSEG\d){0,1}$\s*";
        public const string stationsStopsTextReg = @"^[a-zæøåÆØÅA-Z].*\(([a-zæøåÆØÅA-Z]{2,3})\)";
        /// <summary>
        /// Separator includes: space, coma, colon, semicolon, new line, new paragraph, tab.
        /// </summary>
        public static string[] splitSepar = { "\r\n", ":", ";", ",", "\n", "\t", " " };
}
}
