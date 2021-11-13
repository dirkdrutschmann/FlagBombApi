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
        ///   Status eines Spielerpaares
        /// </summary>
        public enum PlayingStatus
        {
            /// <summary>
            ///   Anfrage gesendet
            /// </summary>
            Requested,

            /// <summary>
            ///   Anfrage angenommen
            /// </summary>
            Accepted,

            /// <summary>
            ///   Anfrage abgelehnt
            /// </summary>
            Rejected,

            /// <summary>
            ///   Im Spiel
            /// </summary>
            InGame
        }
    }
}
