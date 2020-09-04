using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.Extensions
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan? MultipliedBy(this TimeSpan? originalTimeSpan, float multiplier)
        {
            if (originalTimeSpan == null)
            {
                return null;
            }
            return TimeSpan.FromTicks(((TimeSpan)originalTimeSpan).Ticks * (long)multiplier);
        }
    }
}
