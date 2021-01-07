using MiscUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Code.Extensions
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

        public static T GetWeightedRandomKey<T>(this Dictionary<T, double> weightedKeys)
        {
            double totalWeights = weightedKeys.Values.Sum();
            double selectedWeight = StaticRandom.NextDouble() * totalWeights;
            T source = weightedKeys.Keys.FirstOrDefault();
            foreach ((T tempSource, double tempWeight) in weightedKeys)
            {
                source = tempSource;
                selectedWeight -= tempWeight;
                if (selectedWeight <= 0)
                {
                    break;
                }
            }
            return source;
        }
    }
}
