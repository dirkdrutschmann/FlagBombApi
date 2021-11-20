using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Model.Map.Items
{
    /// <summary>
    ///   Oberklasse für alle Items
    /// </summary>
    public class Item
    {
        /// <summary>
        ///   Id des Items
        /// </summary>
        [JsonProperty("itemId")]
        public Guid Id { get; set; }

        /// <summary>
        ///   Quadrat des Items
        /// </summary>
        [JsonProperty("square")]
        public Square Square { get; set; }

        /// <summary>
        ///   Erstellt eine Instanz der Itemsklasse
        /// </summary>
        /// <param name="x">X-Position</param>
        /// <param name="y">y-Position</param>
        /// <param name="width">Breite</param>
        public Item (int x, int y, int width)
        {
            Square = new Square(x, y, width);
            Id = Guid.NewGuid();
        }
    }
}
