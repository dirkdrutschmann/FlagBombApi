using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIPacBomb.Model.Map
{
    public enum Type
    {
        Wall = 0,
        Free = 1    
    }

    public static class Extensions
    {        
        public static Type Random(this Type type)
        {
            Random rnd = new Random();
            
            if (rnd.Next(0, 1) == 0)
            {
                return Type.Wall;
            }
            else
            {
                return Type.Free;
            }
        }
    }
}
