using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Model.Map
{
    public class Square
    {
        [JsonProperty("upperLeft")]
        public Coord UpperLeft { get; set; }

        [JsonProperty("downLeft")]
        public Coord DownLeft { get; set; }

        [JsonProperty("upperRight")]
        public Coord UpperRight { get; set; }

        [JsonProperty("downRight")]
        public Coord DownRight { get; set; }

        public Square (int x, int y, int width)
        {
            DownLeft = new Coord(x, y);
            UpperLeft = new Coord(x, y - width);
            DownRight = new Coord(x + width, y);
            UpperRight = new Coord(x + width, y - width);
        }

    }
}
