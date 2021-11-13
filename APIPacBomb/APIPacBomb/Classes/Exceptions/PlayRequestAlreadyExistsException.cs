using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Classes.Exceptions
{
    /// <summary>
    ///   Ausnahme wenn Spieleanfrage bereits eingeleitet wurde
    /// </summary>
    public class PlayRequestAlreadyExistsException : Exception
    {
        public PlayRequestAlreadyExistsException(string message) : base(message)
        { }
    }
}
