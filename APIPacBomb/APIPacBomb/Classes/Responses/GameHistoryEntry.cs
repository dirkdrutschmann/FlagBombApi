using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Classes.Responses
{
    /// <summary>
    ///   Klasse für einen Eintrag der Spielehistorie
    /// </summary>
    public class GameHistoryEntry
    {
        /// <summary>
        ///   Zeitpunkt der Anfrage
        /// </summary>
        [JsonProperty("requestedOn")]
        public DateTime RequestedOn { get; set; }

        /// <summary>
        ///   Gegner
        /// </summary>
        [JsonProperty("opponent")]
        public string Opponent { get; set; }
    }
}
