using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Classes.Requests
{
    public class MapRequest
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public int SquareFactor { get; set; }
    }
}
