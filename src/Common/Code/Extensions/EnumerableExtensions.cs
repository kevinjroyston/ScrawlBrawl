using System;
using System.Collections.Generic;
using System.Linq;
using Common.DataModels;

namespace Common.Code.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool HasSameElements<T>(this IEnumerable<T> thisEnumerable, IEnumerable<T> otherEnumerable)
            where T : IComparable
        {
            return thisEnumerable.OrderBy(item => item).SequenceEqual(otherEnumerable.OrderBy(item => item));
        }
        public static bool HasSameElements(this IEnumerable<Identifiable> thisEnumerable, IEnumerable<Identifiable> otherEnumerable)
        {
            return thisEnumerable.OrderBy(item => item.Id).Zip(otherEnumerable.OrderBy(item => item.Id)).All(tup => tup.First?.Id != null && tup.Second?.Id!= null && tup.First.Id == tup.Second.Id);
        }
    }
}
