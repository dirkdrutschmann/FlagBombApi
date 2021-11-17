using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Model.Map
{
    /// <summary>
    ///   Klasse für Quadrate
    /// </summary>
    public class Square
    {
        /// <summary>
        ///   Linker oberer Punkt
        /// </summary>
        [JsonProperty("upperLeft")]
        public Coord UpperLeft { get; set; }

        /// <summary>
        ///   Linker unterer Punkt
        /// </summary>
        [JsonProperty("downLeft")]
        public Coord DownLeft { get; set; }

        /// <summary>
        ///   Rechter oberer Punkt
        /// </summary>
        [JsonProperty("upperRight")]
        public Coord UpperRight { get; set; }

        /// <summary>
        ///   Rechter unterer Punkt
        /// </summary>
        [JsonProperty("downRight")]
        public Coord DownRight { get; set; }

        /// <summary>
        ///   Erzeugt eine Instanz der Quatdratsklasse
        /// </summary>
        /// <param name="x">X-Position</param>
        /// <param name="y">Y-Position</param>
        /// <param name="width">Breite</param>
        public Square (int x, int y, int width)
        {
            DownLeft = new Coord(x, y);
            UpperLeft = new Coord(x, y - width);
            DownRight = new Coord(x + width, y);
            UpperRight = new Coord(x + width, y - width);
        }

    }
}
