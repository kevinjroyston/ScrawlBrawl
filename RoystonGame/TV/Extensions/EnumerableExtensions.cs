using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool HasSameElements<T>(this IEnumerable<T> thisEnumerable, IEnumerable<T> otherEnumerable)
        {
            return thisEnumerable.All(element => otherEnumerable.Contains(element)) && otherEnumerable.All(element => thisEnumerable.Contains(element));
        }
    }
}
