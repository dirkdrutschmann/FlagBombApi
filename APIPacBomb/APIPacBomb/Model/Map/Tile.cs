using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Model.Map
{
    public class Tile : Square
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("type")]
        public Type Type { get; set; }

        public Tile(Coord coord, int width, Type type) : base (coord.X, coord.Y, width)
        {
            Width = width;
            Type = type;
        }
    } 
}
