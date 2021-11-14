using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        ///   Status eines Spielerpaares
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum PlayingStatus
        {
            /// <summary>
            ///   Anfrage gesendet
            /// </summary>
            REQUESTED,

            /// <summary>
            ///   Anfrage angenommen
            /// </summary>
            ACCEPTED,

            /// <summary>
            ///   Anfrage abgelehnt
            /// </summary>
            REJECTED,

            /// <summary>
            ///   Im Spiel
            /// </summary>
            IN_GAME,

            /// <summary>
            ///   Spiel vorbei
            /// </summary>
            GAME_OVER
        }
    }
}
