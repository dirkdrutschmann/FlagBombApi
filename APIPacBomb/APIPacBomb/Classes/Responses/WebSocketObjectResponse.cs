using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Classes.Responses
{
    /// <summary>
    ///   Klasse für Nachrichtenfluss über WebSocket
    /// </summary>
    public class WebSocketObjectResponse
    {
        /// <summary>
        ///   Name der Klasse des Objektes
        /// </summary>
        [JsonProperty("class")]
        public string Class { get; set; }

        /// <summary>
        ///   Instanz der Klasse
        /// </summary>
        [JsonProperty("objectValue")]
        public object ObjectValue { get; set; }

        /// <summary>
        ///   Liefert die Instanz der Klasse als JSON-String zurück
        /// </summary>
        /// <returns>JSON-String der Instanz der Klasse</returns>
        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
