using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Code.Extensions
{
    public static class DictionaryExtensions
    {
        public static V GetValueOrDefault<K, V>(this Dictionary<K, V> dictionary, K key, V defaultValue)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            return defaultValue;
        }
    }
}
