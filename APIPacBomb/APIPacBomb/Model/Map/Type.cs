using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Model.Map
{
    /// <summary>
    ///   Typen auf dem Grid
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]  
    public enum Type
    {
        /// <summary>
        ///   Wand
        /// </summary>
        WALL = 0,

        /// <summary>
        ///   Freies Feld
        /// </summary>
        FREE = 1    
    }
}
