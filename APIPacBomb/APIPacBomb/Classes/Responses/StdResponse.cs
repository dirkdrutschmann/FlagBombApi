using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Classes.Responses
{
    /// <summary>
    ///   Klasse für Standard-Antworten vom Server
    /// </summary>
    public class StdResponse
    {
        /// <summary>
        ///   Erfolgsindikator
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }

        /// <summary>
        ///   Nachricht an den Client
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        ///   Erstellt eine Instanz der Standard-Antwort vom Server
        /// </summary>
        public StdResponse() { }

        /// <summary>
        ///   Erstellt eine Instanz der Standard-Antwort vom Server
        /// </summary>
        /// <param name="success">Erfolgsindikator</param>
        /// <param name="message">Mitteilung</param>
        public StdResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
