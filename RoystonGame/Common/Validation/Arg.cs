using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;

namespace RoystonGame.Common.Validation
{
    public static class Arg
    {
        public static void AssertEqual<T>(T obj, T obj2)
        {
            if (!obj.Equals(obj2))
            {
                throw new Exception(Invariant($"Objects are not equal!! Obj1: {obj} | Obj2: {obj2}"));
            }
        }
    }
}
