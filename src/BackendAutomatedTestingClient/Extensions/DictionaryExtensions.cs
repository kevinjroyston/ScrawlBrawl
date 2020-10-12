using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendAutomatedTestingClient.Extensions
{
    public static class DictionaryExtensions
    {
        public static string PrettyPrint<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict)
        {
            var lines = dict.Select(kvp => $"{(kvp.Key.Equals(dict.Keys.First()) ? "\t" : "\t\t")}{kvp.Key}- {kvp.Value}");
            return string.Join(Environment.NewLine, lines);
        }
    }
}
