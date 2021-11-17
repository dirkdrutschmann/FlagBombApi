using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Model.Map
{
    /// <summary>
    ///   Klasse für Flaggen-Darstellung
    /// </summary>
    public class Flag
    {
        /// <summary>
        ///   X-Position
        /// </summary>
        [JsonProperty("x")]
        public int X { get; set; }

        /// <summary>
        ///   Y-Position
        /// </summary>
        [JsonProperty("y")]
        public int Y { get; set; }

        /// <summary>
        ///   Flaggen-Größe
        /// </summary>
        [JsonProperty("flagSize")]
        public int Size { get; set; }

        /// <summary>
        ///   Spielerfarbe
        /// </summary>
        [JsonProperty("color")]
        public PlayerColor Color { get; set; }
    }

}
