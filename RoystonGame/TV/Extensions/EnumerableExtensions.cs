using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool HasSameElements<T>(this IEnumerable<T> thisEnumerable, IEnumerable<T> otherEnumerable, IComparer<T> comparer = null)
        {
            if (comparer != null)
            {
                return thisEnumerable.OrderBy(item => item, comparer).Equals(otherEnumerable.OrderBy(item => item, comparer));
            }
            else
            {
                return thisEnumerable.OrderBy(item => item).Equals(otherEnumerable.OrderBy(item => item));
            }
        }
    }
}
