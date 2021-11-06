using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Model.Map
{
    public class Coord
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        public Coord() { }

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
