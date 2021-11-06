using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Classes
{
    public class TileJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Model.Map.Tile) || objectType == typeof(Model.Map.Wall);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() == typeof(Model.Map.Wall))
            {
                var wall = (JObject)JToken.FromObject(value);
                wall.WriteTo(writer);
            }
            else if (value.GetType() == typeof(Model.Map.Tile))
            {
                var tile = (JObject)JToken.FromObject(value);
                tile.WriteTo(writer);
            }
        }
    }
}
