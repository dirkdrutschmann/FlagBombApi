using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Model.Map
{
    /// <summary>
    ///   Klasse für Bomberman-Darstellung
    /// </summary>
    public class Bomberman
    {
        /// <summary>
        ///   Nutzer-Id
        /// </summary>
        [JsonProperty("userId")]
        public int UserId { get; set; }

        /// <summary>
        ///   Breit  des Bomberman
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }

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
        ///   Größe des Models
        /// </summary>
        [JsonProperty("bomberManSize")]
        public int Size { get; set; }

        /// <summary>
        ///   Eigene Flagge
        /// </summary>
        [JsonProperty("ownedFlag")]
        public Flag OwnedFlag { get; set; }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
