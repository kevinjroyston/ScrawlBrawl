using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Common.Code.Extensions
{
    public static class ConcurrentDictionaryExtensions
    {
        public static void AddOrReplace<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.AddOrUpdate(
                key: key,
                addValue: value,
                updateValueFactory: (TKey updateKey, TValue updateValue) => value);
        }

        public static void AddOrIncrease<TKey>(this ConcurrentDictionary<TKey, int> dictionary, TKey key, int addValue, int increaseAmount)
        {
            dictionary.AddOrUpdate(
                key: key,
                addValue: addValue,
                updateValueFactory: (TKey updateKey, int updateValue) => updateValue + increaseAmount);
        }
        public static void AddOrAppend<TKey, TListValue>(this ConcurrentDictionary<TKey, List<TListValue>> dictionary, TKey key, TListValue addValue)
        {
            dictionary.AddOrUpdate(
                key: key,
                addValue: new List<TListValue>() { addValue},
                updateValueFactory: (TKey updateKey, List<TListValue> oldList) =>
                {
                    oldList.Add(addValue);
                    return oldList;
                });
        }

        public static void AddOrAppend<TKey, TListValue>(this ConcurrentDictionary<TKey, ConcurrentBag<TListValue>> dictionary, TKey key, TListValue addValue)
        {
            dictionary.AddOrUpdate(
                key: key,
                addValue: new ConcurrentBag<TListValue>() { addValue },
                updateValueFactory: (TKey updateKey, ConcurrentBag<TListValue> oldList) =>
                {
                    oldList.Add(addValue);
                    return oldList;
                });
        }
    }
}
