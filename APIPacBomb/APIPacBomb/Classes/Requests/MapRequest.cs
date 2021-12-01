using Newtonsoft.Json;
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
        [JsonProperty("width")]
        public int Width { get; set; }

        /// <summary>
        ///   Spielfeldhoehe
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; }

        /// <summary>
        ///   Groesse der Kacheln
        /// </summary>
        [JsonProperty("squareFactor")]
        public int SquareFactor { get; set; }

        /// <summary>
        ///   Anzahl der Capture-Events
        /// </summary>
        [JsonProperty("captureFlagCount")]
        public int CaptureFlagCount { get; set; }

        /// <summary>
        ///   Bombenanzahl beim Start
        /// </summary>
        [JsonProperty("bombsAtStart")]
        public int BombsAtStart { get; set; }
    }
}
