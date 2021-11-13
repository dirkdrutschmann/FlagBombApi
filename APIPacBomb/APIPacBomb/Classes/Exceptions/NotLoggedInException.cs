﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Classes.Exceptions
{
    /// <summary>
    ///   Ausnahme wenn Nutzer nicht angemeldet ist
    /// </summary>
    public class NotLoggedInException : Exception
    {
        public NotLoggedInException(string message): base(message)
        { }
    }
}
