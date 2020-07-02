using System;
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
    }
}
