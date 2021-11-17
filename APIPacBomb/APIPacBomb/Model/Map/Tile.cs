using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Model.Map
{
    /// <summary>
    ///   Klasse für Kacheln
    /// </summary>
    public class Tile : Square
    {
        private Items.Gem _Item;

        /// <summary>
        ///   Kachelbreite
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }

        /// <summary>
        ///   Kacheltyp
        /// </summary>
        [JsonProperty("type")]
        public Type Type { get; set; }

        /// <summary>
        ///   Indikator, ob Feld mit einem Item belegt ist
        /// </summary>
        [JsonIgnore]
        public bool HasItem { get; set; }

        /// <summary>
        ///   Item auf der Kachel
        /// </summary>
        [JsonIgnore]
        public Items.Gem Item
        {
            get
            {
                return _Item;
            }

            set
            {
                _Item = value;

                if (value == null)
                {
                    HasItem = false;
                }
                else
                {
                    HasItem = true;
                }                
            }
        }

        /// <summary>
        ///   Erzeugt eine Instanz einer Kachel
        /// </summary>
        /// <param name="coord">Linker unterer Punkt</param>
        /// <param name="width">Breite</param>
        /// <param name="type">Typ</param>
        public Tile(Coord coord, int width, Type type) : base (coord.X, coord.Y, width)
        {
            Width = width;
            Type = type;
        }
    } 
}
