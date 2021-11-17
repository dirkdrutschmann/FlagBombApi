using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Classes
{
    /// <summary>
    ///  Eventargumente, die bei Generierung eines neuen Items übergeben werden
    /// </summary>
    public class ItemGeneratedEventArgs : EventArgs
    {
        /// <summary>
        ///   Neue Map
        /// </summary>
        public Model.Map.Grid Grid { get; set; }

        /// <summary>
        ///   Neu generiete Belohnung
        /// </summary>
        public Model.Map.Items.Gem NewGem { get; set; }
    }
}
