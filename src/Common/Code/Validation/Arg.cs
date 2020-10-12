using System;
using static System.FormattableString;

namespace Common.Code.Validation
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
        public static void NotNull<T>(T param, string paramName = null) where T : class
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
