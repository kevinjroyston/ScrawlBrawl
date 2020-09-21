using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoystonGameAutomatedTestingClient.Extensions
{
    public static class DictionaryExtensions
    {
        public static string PrettyPrint<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict)
        {
            var lines = dict.Select(kvp => $"\t{kvp.Key}- {kvp.Value}");
            return string.Join(Environment.NewLine, lines);
        }
    }
}
