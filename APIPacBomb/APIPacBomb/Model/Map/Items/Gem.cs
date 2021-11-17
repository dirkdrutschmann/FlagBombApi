using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Model.Map.Items
{
    /// <summary>
    ///   Klasse für Belohnungen
    /// </summary>
    public class Gem : Item
    {
        /// <summary>
        ///   Bild-Index zwischen 1 und 3
        /// </summary>
        [JsonProperty("imageIndex")]
        public int ImageIndex { get; set; }

        /// <summary>
        ///   Zeile in der das Item sitzt
        /// </summary>
        [JsonIgnore]
        public int Row { get; set; }

        /// <summary>
        ///   Spalte in der das Item sitzt
        /// </summary>
        [JsonIgnore]
        public int Column { get; set; }

        /// <summary>
        ///   Erstellt eine Instanz eines Items für Belohnungen
        /// </summary>
        /// <param name="x">X-Position</param>
        /// <param name="y">Y-Position</param>
        /// <param name="width">Breite</param>
        /// <param name="imageIndex">Bild-Index zwischen 1 und 3</param>
        public Gem(int x, int y, int width, int imageIndex) : base (x, y, width)
        {
            ImageIndex = imageIndex;                
        }

        /// <summary>
        ///   Erzeugt eine Belohnung auf einem zufälligen freien Feld auf der Karte
        /// </summary>
        /// <param name="map">Karte</param>
        /// <returns>
        ///   Feld, wenn ein freier Platz in einem festgelegten Intervall bestimmt 
        ///   werden konnte, sonst null
        /// </returns>
        public static Gem GenerateRandom(Grid map)
        {
            Random random = new Random();
            int boundery = 0;

            while (boundery < 100)
            {
                int row = random.Next(0, map.RowCount);
                int column = random.Next(0, map.ColumnCount);

                if (map.Columns[column][row].Type == Type.FREE)
                {
                    return new Gem(
                        map.Columns[column][row].DownLeft.X,
                        map.Columns[column][row].DownLeft.Y,
                        map.Columns[column][row].Width,
                        random.Next(1, 3)
                    )
                    {
                        Row = row,
                        Column = column
                    };
                }

                boundery++;
            }

            return null;
        }
    }
}
