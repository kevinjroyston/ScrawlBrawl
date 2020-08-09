using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.Helpers.Extensions
{
    public static class CollectionExtensions
    {
        public static int? IndexOf<T>(this IReadOnlyList<T> list, T element, int? defaultValue = null)
        {
            int i = 0;
            foreach (T ele in list)
            {
                if (Equals(ele, element))
                {
                    return i;
                }
                i++;
            }
            return defaultValue;
        }
        public static int? FirstIndex<T>(this IReadOnlyList<T> list, Func<T, bool> elementSelector, int? defaultValue = null)
        {
            int i = 0;
            foreach (T ele in list)
            {
                if (elementSelector(ele))
                {
                    return i;
                }
                i++;
            }
            return defaultValue;
        }
    }
}
