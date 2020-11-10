using MiscUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Code.Helpers
{
    public static class MathHelpers
    {
        public static T GetWeightedRandom<T>(IDictionary<T, int> valueToWeight)
        {
            int totalWeight = valueToWeight.Values.Sum();
            int random = StaticRandom.Next(0, totalWeight);
            foreach (T value in valueToWeight.Keys)
            {
                if (random <= valueToWeight[value])
                {
                    return value;
                }
                else
                {
                    random -= valueToWeight[value];
                }
            }
            throw new Exception("Something went wrong getting weighted random");
        }

        public static double ThreePointLerp(double minX, double aveX, double maxX, double x, double minValue, double aveValue, double maxValue)
        {
            if (x < minX)
            {
                return minValue;
            }
            else if (x < aveX)
            {
                return Lerp(minValue, aveValue, (x - minX) / (aveX - minX));
            }
            else if (x < maxX)
            {
                return Lerp(aveValue, maxValue, (x - aveX) / (maxX - aveX));
            }
            else
            {
                return maxValue;
            }
        }
        public static double Lerp(double a, double b, double t)
        {
            return (b - a) * t + a;
        }
        public static double ClampedLerp(double a, double b, double t)
        {
            t = Math.Clamp(t, 0.0, 1.0);
            return (b - a) * t + a;
        }
    }
}
