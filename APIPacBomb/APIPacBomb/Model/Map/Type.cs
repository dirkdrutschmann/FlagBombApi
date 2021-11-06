using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Model.Map
{
    [JsonConverter(typeof(StringEnumConverter))]  
    public enum Type
    {
        WALL = 0,
        FREE = 1    
    }
}
