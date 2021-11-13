using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Classes
{
    /// <summary>
    ///   Klasse der Nutzeranfragen-Paare
    /// </summary>
    public partial class UserPlayingPair
    {
        /// <summary>
        ///   Eindeutige Id des Spielerpaares
        /// </summary>
        [JsonProperty("id")]
        public Guid Id { get; set; }

        /// <summary>
        ///   Anfragender Nutzer
        /// </summary>
        [JsonProperty("requestingUser")]
        public Model.User RequestingUser { get; set; }

        /// <summary>
        ///   Angefragter Nutzer
        /// </summary>
        [JsonProperty("requestedUser")]
        public Model.User RequestedUser { get; set; }

        /// <summary>
        ///   Zeitpunkt der Anfragestellung
        /// </summary>
        [JsonProperty("requestTime")]
        public DateTime RequestTime { get; private set; }

        /// <summary>
        ///   Status des Paares
        /// </summary>
        [JsonProperty("status")]
        public PlayingStatus Status { get; set; }

        /// <summary>
        ///   Erstellt eine Instanz der UserPlayingPairs-Klasse
        /// </summary>
        public UserPlayingPair()
        {
            Id = Guid.NewGuid();
            RequestTime = DateTime.Now;
            Status = PlayingStatus.REQUESTED;
        }

        /// <summary>
        ///   Erstellt eine Instanz der UserPlayingPairs-Klasse
        /// </summary>
        /// <param name="requestingUser">Anfragender Nutzer</param>
        /// <param name="requestedUser">Angefragter Nutzer</param>
        public UserPlayingPair(Model.User requestingUser, Model.User requestedUser)
        {
            Id = Guid.NewGuid();
            RequestingUser = requestingUser;
            RequestedUser = requestedUser;
            RequestTime = DateTime.Now;
            Status = PlayingStatus.REQUESTED;
        }
    }
}
