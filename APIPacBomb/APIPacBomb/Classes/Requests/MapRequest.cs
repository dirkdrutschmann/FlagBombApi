using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Classes.Requests
{
    /// <summary>
    ///   Body für Spielanfrage
    /// </summary>
    public class MapRequest
    {
        /// <summary>
        ///   Spielfeldbreite
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        ///   Spielfeldhoehe
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        ///   Groesse der Kacheln
        /// </summary>
        public int SquareFactor { get; set; }

        /// <summary>
        ///   Anzahl der Capture-Events
        /// </summary>
        public int CaptureFlagCount { get; set; }
    }
}
