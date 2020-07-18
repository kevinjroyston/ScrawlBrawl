using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;

namespace RoystonGame.TV.GameModes.Common
{
    public static class CommonHelpers
    {
        public static string HtmlImageWrapper(string image, int width = 240, int height = 240)
        {
            return Invariant($"<img width=\"{width}\" height=\"{height}\" src=\"{image}\"/>");
        }

        public static int PointsForSpeed(int maxPoints, int minPoints, double startTime, double endTime, double secondsTaken)
        {
            if (secondsTaken <= startTime)
            {
                return maxPoints;
            }
            if(secondsTaken >= endTime)
            {
                return minPoints;
            }
            else
            {
                return (int)((endTime - secondsTaken)/(endTime - startTime) * (maxPoints - minPoints)) + minPoints;
            }
        }

        public static double LinearMapping(double minPosition, double maxPosition, double position, double minValue, double maxValue)
        {
            return ((maxPosition - position) / (maxPosition - minPosition) * (maxValue - minValue)) + minValue;
        }

        /// <summary>
        /// Evenly distributes the elements of toDistribute into the groups. Will duplicate toDistribute elements
        /// until the groups have reached maxGroupSize or all elements that pass the check have been assigned for each group
        /// </summary>
        public static Dictionary<TGroup, List<TToDistribute>> EvenlyDistribute<TGroup, TToDistribute>(
            List<TGroup> groups,
            List<TToDistribute> toDistribute,
            int maxGroupSize,
            Func<TGroup, TToDistribute, bool> validDistributeCheck)
        {
            if (groups.Count < 2)
            {
                throw new Exception("groups must have at least 2 elements");
            }

            Random Rand = new Random();

            List<TToDistribute> skipedDistributes = new List<TToDistribute>();
            int distributeIndex = 0;
            Dictionary<TGroup, List<TToDistribute>> distributedDict = new Dictionary<TGroup, List<TToDistribute>>();

            foreach (TGroup group in groups) 
            {
                distributedDict.Add(group, new List<TToDistribute>());
                for (int i = 0; i < toDistribute.Count; i++)
                {
                    if (distributedDict[group].Count >= maxGroupSize)
                    {
                        break;
                    }
                    bool addedFromSkiped = false;
                    foreach (TToDistribute skiped in skipedDistributes)
                    {
                        if (validDistributeCheck(group, skiped) && !distributedDict[group].Contains(skiped))
                        {
                            distributedDict[group].Add(skiped);
                            skipedDistributes.Remove(skiped);
                            addedFromSkiped = true;
                            break;
                        }
                    }
                    if (!addedFromSkiped)
                    {
                        if (validDistributeCheck(group, toDistribute[distributeIndex]) && !distributedDict[group].Contains(toDistribute[distributeIndex]))
                        {
                            distributedDict[group].Add(toDistribute[distributeIndex]);
                        }
                        else
                        {
                            skipedDistributes.Add(toDistribute[distributeIndex]);
                        }
                    }

                    distributeIndex++;
                    if (distributeIndex >= toDistribute.Count)
                    {
                        distributeIndex = 0;
                    }
                }
            }
            if (distributedDict.Count != groups.Count)
            {
                throw new Exception("Something went wrong assigning into groups");
            }
            // Make 100 attempts at valid swaps
            for (int i = 0; i < 100; i++)
            {
                TGroup randGroup1 = distributedDict.Keys.ToList()[Rand.Next(0, distributedDict.Count)];
                TGroup randGroup2 = distributedDict.Keys.Where((TGroup group) => !group.Equals(randGroup1)).ToList()[Rand.Next(0, distributedDict.Count - 1)];

                TToDistribute randDistribute1 = distributedDict[randGroup1][Rand.Next(0, distributedDict[randGroup1].Count)];
                TToDistribute randDistribute2 = distributedDict[randGroup2][Rand.Next(0, distributedDict[randGroup2].Count)];

                // Determines if a swap is valid
                if (
                    validDistributeCheck(randGroup1, randDistribute2) 
                    && validDistributeCheck(randGroup2, randDistribute1)
                    && !distributedDict[randGroup1].Contains(randDistribute2)
                    && !distributedDict[randGroup2].Contains(randDistribute1))
                {
                    distributedDict[randGroup1].Remove(randDistribute1);
                    distributedDict[randGroup1].Add(randDistribute2);

                    distributedDict[randGroup2].Remove(randDistribute2);
                    distributedDict[randGroup2].Add(randDistribute1);
                }
            }

            return distributedDict;
        }
    }
}
