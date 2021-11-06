using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Model.Map
{
    public class Wall : Tile
    {
        [JsonProperty("isDestroyable")]
        public bool IsDestroyable { get; private set; }

        public Wall(Coord coord, int width, bool isDestroyable) : base (coord, width, Type.WALL)
        {
            IsDestroyable = isDestroyable;
        }
    }
}
